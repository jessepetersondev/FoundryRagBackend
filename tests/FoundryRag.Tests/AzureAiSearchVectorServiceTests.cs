using FluentAssertions;
using FoundryRag.Api.Infrastructure;
using FoundryRag.Api.Models;
using FoundryRag.Api.Options;
using FoundryRag.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FoundryRag.Tests;

public sealed class AzureAiSearchVectorServiceTests
{
    [Fact]
    public void MeetsMinScoreThreshold_NullScoreWithPositiveThreshold_IsFilteredOut()
    {
        AzureAiSearchVectorService.MeetsMinScoreThreshold(null, 0.7).Should().BeFalse();
    }

    [Fact]
    public void MeetsMinScoreThreshold_NullScoreWithZeroThreshold_IsAllowed()
    {
        AzureAiSearchVectorService.MeetsMinScoreThreshold(null, 0).Should().BeTrue();
    }

    [Fact]
    public void MeetsMinScoreThreshold_LowScore_IsFilteredOut()
    {
        AzureAiSearchVectorService.MeetsMinScoreThreshold(0.5, 0.7).Should().BeFalse();
    }

    [Fact]
    public void MeetsMinScoreThreshold_HighScore_IsRetained()
    {
        AzureAiSearchVectorService.MeetsMinScoreThreshold(0.8, 0.7).Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_QueryEmbeddingDimensionMismatch_ThrowsSafeConfigurationError()
    {
        var sut = CreateService(embeddingDimensions: 3);

        var act = () => sut.SearchAsync([0.1f, 0.2f], 5, CancellationToken.None);

        await act.Should().ThrowAsync<ConfigurationMissingException>()
            .WithMessage("*Rag:EmbeddingDimensions*");
    }

    [Fact]
    public async Task UploadDocumentsAsync_DocumentEmbeddingDimensionMismatch_ThrowsSafeConfigurationError()
    {
        var sut = CreateService(embeddingDimensions: 3);
        var document = new IndexedDocument(
            "market-001",
            "CPI market",
            "Economics",
            "Content",
            "sample",
            "2026-01-01",
            [0.1f, 0.2f]);

        var act = () => sut.UploadDocumentsAsync([document], CancellationToken.None);

        await act.Should().ThrowAsync<ConfigurationMissingException>()
            .WithMessage("*Rag:EmbeddingDimensions*");
    }

    private static AzureAiSearchVectorService CreateService(int embeddingDimensions)
    {
        var searchFactory = new AzureSearchClientFactory(Options.Create(new AzureSearchOptions
        {
            Endpoint = "https://example.search.windows.net",
            ApiKey = "placeholder",
            IndexName = "market-rag-index"
        }));

        return new AzureAiSearchVectorService(
            searchFactory,
            new RetryPolicy(NullLogger<RetryPolicy>.Instance),
            Options.Create(new RagOptions { EmbeddingDimensions = embeddingDimensions }),
            NullLogger<AzureAiSearchVectorService>.Instance);
    }
}
