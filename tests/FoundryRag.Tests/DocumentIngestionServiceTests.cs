using FluentAssertions;
using FoundryRag.Api.Infrastructure;
using FoundryRag.Api.Models;
using FoundryRag.Api.Options;
using FoundryRag.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FoundryRag.Tests;

public sealed class DocumentIngestionServiceTests
{
    [Fact]
    public async Task IngestSeedDataAsync_ReadsEmbedsUploadsAndReturnsCounts()
    {
        var seedReader = Substitute.For<ISeedDataReader>();
        seedReader.ReadSeedDataAsync(Arg.Any<CancellationToken>())
            .Returns([CreateMarketDocument("market-001"), CreateMarketDocument("market-002")]);

        var embedding = Substitute.For<IEmbeddingService>();
        embedding.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([0.1f, 0.2f, 0.3f]);

        var vectorSearch = Substitute.For<IVectorSearchService>();
        IReadOnlyList<IndexedDocument>? uploaded = null;
        vectorSearch
            .UploadDocumentsAsync(Arg.Do<IReadOnlyList<IndexedDocument>>(docs => uploaded = docs), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var sut = new DocumentIngestionService(
            seedReader,
            embedding,
            vectorSearch,
            Options.Create(new AzureSearchOptions
            {
                Endpoint = "https://example.search.windows.net",
                ApiKey = "placeholder",
                IndexName = "market-rag-index"
            }),
            Options.Create(new RagOptions { EmbeddingDimensions = 3 }),
            NullLogger<DocumentIngestionService>.Instance);

        var response = await sut.IngestSeedDataAsync(CancellationToken.None);

        response.DocumentsRead.Should().Be(2);
        response.DocumentsUploaded.Should().Be(2);
        response.IndexName.Should().Be("market-rag-index");
        await embedding.Received(2).GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await vectorSearch.Received(1).EnsureIndexCreatedAsync(Arg.Any<CancellationToken>());
        uploaded.Should().NotBeNull();
        uploaded!.Should().HaveCount(2);
        uploaded![0].Content.Should().Contain("Title:");
        uploaded![0].Embedding.Should().Equal(0.1f, 0.2f, 0.3f);
    }

    [Fact]
    public async Task IngestSeedDataAsync_EmbeddingDimensionMismatch_ThrowsSafeConfigurationError()
    {
        var seedReader = Substitute.For<ISeedDataReader>();
        seedReader.ReadSeedDataAsync(Arg.Any<CancellationToken>())
            .Returns([CreateMarketDocument("market-001")]);

        var embedding = Substitute.For<IEmbeddingService>();
        embedding.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([0.1f, 0.2f]);

        var vectorSearch = Substitute.For<IVectorSearchService>();

        var sut = new DocumentIngestionService(
            seedReader,
            embedding,
            vectorSearch,
            Options.Create(new AzureSearchOptions
            {
                Endpoint = "https://example.search.windows.net",
                ApiKey = "placeholder",
                IndexName = "market-rag-index"
            }),
            Options.Create(new RagOptions { EmbeddingDimensions = 3 }),
            NullLogger<DocumentIngestionService>.Instance);

        var act = () => sut.IngestSeedDataAsync(CancellationToken.None);

        await act.Should().ThrowAsync<ConfigurationMissingException>()
            .WithMessage("*Rag:EmbeddingDimensions*");
        await vectorSearch.DidNotReceive().UploadDocumentsAsync(Arg.Any<IReadOnlyList<IndexedDocument>>(), Arg.Any<CancellationToken>());
    }

    private static MarketDocument CreateMarketDocument(string id)
    {
        return new MarketDocument
        {
            Id = id,
            Title = "Will CPI exceed 3%?",
            Category = "Economics",
            Description = "Fictional CPI market.",
            Rules = "Resolves from a sample report.",
            Outcomes = ["Yes", "No"],
            Source = "sample",
            EffectiveDate = "2026-03-01"
        };
    }
}
