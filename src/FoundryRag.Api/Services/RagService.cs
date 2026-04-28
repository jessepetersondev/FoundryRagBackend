using FoundryRag.Api.Contracts;
using FoundryRag.Api.Infrastructure;
using FoundryRag.Api.Models;
using FoundryRag.Api.Options;
using Microsoft.Extensions.Options;

namespace FoundryRag.Api.Services;

public sealed class RagService : IRagService
{
    public const string InsufficientInformationAnswer = "I do not have enough information in the indexed data to answer that.";

    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorSearchService _vectorSearchService;
    private readonly IPromptBuilder _promptBuilder;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly IAnswerGroundingValidator _answerGroundingValidator;
    private readonly RagOptions _options;
    private readonly ILogger<RagService> _logger;

    public RagService(
        IEmbeddingService embeddingService,
        IVectorSearchService vectorSearchService,
        IPromptBuilder promptBuilder,
        IChatCompletionService chatCompletionService,
        IAnswerGroundingValidator answerGroundingValidator,
        IOptions<RagOptions> options,
        ILogger<RagService> logger)
    {
        _embeddingService = embeddingService;
        _vectorSearchService = vectorSearchService;
        _promptBuilder = promptBuilder;
        _chatCompletionService = chatCompletionService;
        _answerGroundingValidator = answerGroundingValidator;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AskResponse> AskAsync(AskRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var question = ValidateQuestion(request.Question);
        var topK = ResolveTopK(request.TopK);

        _logger.LogInformation("Received RAG query with length {QuestionLength} and topK {TopK}", question.Length, topK);

        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(question, cancellationToken);
        var retrievedDocuments = await _vectorSearchService.SearchAsync(queryEmbedding, topK, cancellationToken);

        _logger.LogInformation(
            "Vector retrieval returned {DocumentCount} documents: {DocumentScores}",
            retrievedDocuments.Count,
            retrievedDocuments.Select(d => new { d.Id, d.Score }));

        if (retrievedDocuments.Count == 0)
        {
            _logger.LogInformation("Returning insufficiency response because retrieval returned no usable documents");
            return new AskResponse(
                InsufficientInformationAnswer,
                Array.Empty<SourceReference>(),
                new RetrievalMetadata(topK, 0));
        }

        var prompt = _promptBuilder.BuildPrompt(question, retrievedDocuments);
        _logger.LogInformation("Prompt built with {DocumentCount} retrieved documents", retrievedDocuments.Count);

        var answer = await _chatCompletionService.GenerateAnswerAsync(prompt, cancellationToken);
        _logger.LogInformation("Chat completion returned answer length {AnswerLength}", answer.Length);

        var safeAnswer = ValidateAnswerGrounding(answer, retrievedDocuments);

        return new AskResponse(
            safeAnswer,
            retrievedDocuments.Select(ToSourceReference).ToArray(),
            new RetrievalMetadata(topK, retrievedDocuments.Count));
    }

    private string ValidateQuestion(string? question)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new RequestValidationException("Question is required.");
        }

        var trimmed = question.Trim();
        if (trimmed.Length > _options.MaxQuestionLength)
        {
            throw new RequestValidationException($"Question must be {_options.MaxQuestionLength} characters or fewer.");
        }

        return trimmed;
    }

    private int ResolveTopK(int? requestedTopK)
    {
        var topK = requestedTopK ?? _options.DefaultTopK;
        topK = Math.Max(1, topK);
        topK = Math.Min(_options.MaxTopK, topK);
        return topK;
    }

    private static SourceReference ToSourceReference(RetrievedDocument document) =>
        new(document.Id, document.Title, document.Category, document.Source, document.Score);

    private string ValidateAnswerGrounding(string answer, IReadOnlyList<RetrievedDocument> retrievedDocuments)
    {
        if (string.IsNullOrWhiteSpace(answer))
        {
            _logger.LogWarning("Returning guarded answer because chat completion returned an empty answer");
            return InsufficientInformationAnswer;
        }

        var validation = _answerGroundingValidator.Validate(answer, retrievedDocuments);
        if (validation.IsValid)
        {
            return answer;
        }

        if (!validation.HasCitations)
        {
            _logger.LogWarning("Returning guarded answer because chat completion did not cite retrieved source IDs");
        }
        else
        {
            _logger.LogWarning(
                "Returning guarded answer because chat completion cited unknown source IDs: {UnknownSourceIds}",
                validation.UnknownSourceIds);
        }

        return BuildCitedInsufficiencyAnswer(retrievedDocuments);
    }

    private static string BuildCitedInsufficiencyAnswer(IReadOnlyList<RetrievedDocument> retrievedDocuments)
    {
        var sourceIds = retrievedDocuments
            .Select(document => document.Id)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .Select(id => $"[{id}]")
            .ToArray();

        return sourceIds.Length == 0
            ? InsufficientInformationAnswer
            : $"{InsufficientInformationAnswer} Relevant retrieved source{(sourceIds.Length == 1 ? string.Empty : "s")}: {string.Join(" ", sourceIds)}.";
    }
}
