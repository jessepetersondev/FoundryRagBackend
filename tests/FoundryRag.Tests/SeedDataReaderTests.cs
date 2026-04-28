using FluentAssertions;
using FoundryRag.Api.Infrastructure;
using FoundryRag.Api.Services;

namespace FoundryRag.Tests;

public sealed class SeedDataReaderTests
{
    [Fact]
    public async Task ReadSeedDataAsync_ReadsValidDocuments()
    {
        using var workspace = SeedWorkspace.Create("""
            [
              {
                "id": "market-001",
                "title": "CPI market",
                "category": "Economics",
                "description": "Sample description",
                "rules": "Sample rules",
                "outcomes": ["Yes", "No"],
                "source": "sample",
                "effectiveDate": "2026-01-01"
              }
            ]
            """);
        var sut = new SeedDataReader(workspace.Environment);

        var documents = await sut.ReadSeedDataAsync(CancellationToken.None);

        documents.Should().ContainSingle().Which.Id.Should().Be("market-001");
    }

    [Fact]
    public async Task ReadSeedDataAsync_MissingFileThrowsClearException()
    {
        using var workspace = SeedWorkspace.CreateWithoutFile();
        var sut = new SeedDataReader(workspace.Environment);

        var act = () => sut.ReadSeedDataAsync(CancellationToken.None);

        await act.Should().ThrowAsync<SeedDataException>()
            .WithMessage("Seed data could not be loaded.");
    }

    [Fact]
    public async Task ReadSeedDataAsync_MalformedJsonThrowsClearException()
    {
        using var workspace = SeedWorkspace.Create("""
            [
              { "id": "market-001",
            ]
            """);
        var sut = new SeedDataReader(workspace.Environment);

        var act = () => sut.ReadSeedDataAsync(CancellationToken.None);

        await act.Should().ThrowAsync<SeedDataException>()
            .WithMessage("Seed data file is not valid JSON.");
    }

    [Fact]
    public async Task ReadSeedDataAsync_MissingRequiredFieldsThrowsClearException()
    {
        using var workspace = SeedWorkspace.Create("""
            [
              {
                "id": "market-001",
                "title": "",
                "category": "Economics",
                "description": "Sample description",
                "rules": "Sample rules",
                "outcomes": ["Yes"],
                "source": "sample",
                "effectiveDate": "2026-01-01"
              }
            ]
            """);
        var sut = new SeedDataReader(workspace.Environment);

        var act = () => sut.ReadSeedDataAsync(CancellationToken.None);

        await act.Should().ThrowAsync<SeedDataException>()
            .WithMessage("*missing required fields*");
    }

    [Fact]
    public async Task ReadSeedDataAsync_DuplicateIdsThrowClearException()
    {
        using var workspace = SeedWorkspace.Create("""
            [
              {
                "id": "market-001",
                "title": "CPI market",
                "category": "Economics",
                "description": "Sample description",
                "rules": "Sample rules",
                "outcomes": ["Yes"],
                "source": "sample",
                "effectiveDate": "2026-01-01"
              },
              {
                "id": "market-001",
                "title": "Fed market",
                "category": "Economics",
                "description": "Sample description",
                "rules": "Sample rules",
                "outcomes": ["Yes"],
                "source": "sample",
                "effectiveDate": "2026-01-01"
              }
            ]
            """);
        var sut = new SeedDataReader(workspace.Environment);

        var act = () => sut.ReadSeedDataAsync(CancellationToken.None);

        await act.Should().ThrowAsync<SeedDataException>()
            .WithMessage("*Duplicate*");
    }
}
