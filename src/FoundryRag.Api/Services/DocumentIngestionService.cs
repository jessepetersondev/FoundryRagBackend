using System.Text;
using FoundryRag.Api.Contracts;
using FoundryRag.Api.Infrastructure;
using FoundryRag.Api.Models;
using FoundryRag.Api.Options;
using Microsoft.Extensions.Options;

namespace FoundryRag.Api.Services;

public sealed class DocumentIngestionService : IDocumentIngestionService
{
    private readonly ISeedDataReader _seedDataReader;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorSearchService _vectorSearchService;
    private readonly AzureSearchOptions _searchOptions;
    private readonly RagOptions _ragOptions;
    private readonly ILogger<DocumentIngestionService> _logger;

    public DocumentIngestionService(
        ISeedDataReader seedDataReader,
        IEmbeddingService embeddingService,
        IVectorSearchService vectorSearchService,
        IOptions<AzureSearchOptions> searchOptions,
        IOptions<RagOptions> ragOptions,
        ILogger<DocumentIngestionService> logger)
    {
        _seedDataReader = seedDataReader;
        _embeddingService = embeddingService;
        _vectorSearchService = vectorSearchService;
        _searchOptions = searchOptions.Value;
        _ragOptions = ragOptions.Value;
        _logger = logger;
    }

    public async Task<IngestResponse> IngestSeedDataAsync(CancellationToken cancellationToken)
    {
        var seedDocuments = await _seedDataReader.ReadSeedDataAsync(cancellationToken);
        var indexedDocuments = new List<IndexedDocument>(seedDocuments.Count);

        foreach (var document in seedDocuments)
        {
            var content = BuildSearchableContent(document);
            var embedding = await _embeddingService.GenerateEmbeddingAsync(content, cancellationToken);
            ValidateEmbeddingDimensions(document.Id, embedding);
            indexedDocuments.Add(new IndexedDocument(
                document.Id,
                document.Title,
                document.Category,
                content,
                document.Source,
                document.EffectiveDate ?? document.CloseDate,
                embedding));
        }

        await _vectorSearchService.EnsureIndexCreatedAsync(cancellationToken);
        await _vectorSearchService.UploadDocumentsAsync(indexedDocuments, cancellationToken);

        _logger.LogInformation("Ingested {DocumentCount} seed documents into index {IndexName}", indexedDocuments.Count, _searchOptions.IndexName);
        return new IngestResponse(seedDocuments.Count, indexedDocuments.Count, _searchOptions.IndexName);
    }

    private static string BuildSearchableContent(MarketDocument document)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Title: {document.Title}");
        builder.AppendLine($"Category: {document.Category}");
        builder.AppendLine($"Description: {document.Description}");
        builder.AppendLine($"Rules: {document.Rules}");
        builder.AppendLine($"Outcomes: {string.Join(", ", document.Outcomes)}");
        builder.AppendLine($"Dates: Effective {document.EffectiveDate ?? "n/a"}; Close {document.CloseDate ?? "n/a"}");

        if (!string.IsNullOrWhiteSpace(document.Notes))
        {
            builder.AppendLine($"Notes: {document.Notes}");
        }

        return builder.ToString();
    }

    private void ValidateEmbeddingDimensions(string documentId, IReadOnlyList<float> embedding)
    {
        if (embedding.Count != _ragOptions.EmbeddingDimensions)
        {
            throw new ConfigurationMissingException(
                $"Embedding dimension count for seed document '{documentId}' does not match configured Rag:EmbeddingDimensions.");
        }
    }
}
