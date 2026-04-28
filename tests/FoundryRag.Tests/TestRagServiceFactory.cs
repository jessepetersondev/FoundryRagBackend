using FoundryRag.Api.Models;
using FoundryRag.Api.Options;
using FoundryRag.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FoundryRag.Tests;

internal static class TestRagServiceFactory
{
    public static RagService Create(
        int maxQuestionLength = 100,
        IVectorSearchService? vectorSearch = null,
        IChatCompletionService? chat = null,
        IAnswerGroundingValidator? answerGroundingValidator = null)
    {
        var embedding = Substitute.For<IEmbeddingService>();
        embedding.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([0.1f, 0.2f, 0.3f]);

        vectorSearch ??= CreateVectorSearch();

        var promptBuilder = Substitute.For<IPromptBuilder>();
        promptBuilder.BuildPrompt(Arg.Any<string>(), Arg.Any<IReadOnlyList<RetrievedDocument>>())
            .Returns(new RagPrompt("system", "user"));

        if (chat is null)
        {
            chat = Substitute.For<IChatCompletionService>();
            chat.GenerateAnswerAsync(Arg.Any<RagPrompt>(), Arg.Any<CancellationToken>())
                .Returns("Grounded answer. [market-001]");
        }

        answerGroundingValidator ??= new AnswerGroundingValidator();

        return new RagService(
            embedding,
            vectorSearch,
            promptBuilder,
            chat,
            answerGroundingValidator,
            Options.Create(new RagOptions { DefaultTopK = 5, MaxTopK = 10, MaxQuestionLength = maxQuestionLength }),
            NullLogger<RagService>.Instance);
    }

    public static IVectorSearchService CreateVectorSearch()
    {
        var vectorSearch = Substitute.For<IVectorSearchService>();
        vectorSearch.SearchAsync(Arg.Any<IReadOnlyList<float>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([new RetrievedDocument("market-001", "CPI market", "Economics", "Content", "sample", 0.9)]);
        return vectorSearch;
    }

    public static async Task ReceivedSearchAsync(this IVectorSearchService vectorSearch, int topK)
    {
        await vectorSearch.Received(1).SearchAsync(Arg.Any<IReadOnlyList<float>>(), topK, Arg.Any<CancellationToken>());
    }
}
