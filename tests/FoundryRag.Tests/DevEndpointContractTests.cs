using FluentAssertions;
using FoundryRag.Api.Contracts;
using FoundryRag.Api.Controllers;
using FoundryRag.Api.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace FoundryRag.Tests;

public sealed class DevEndpointContractTests
{
    [Fact]
    public void Health_ReturnsOkStatus()
    {
        var controller = new HealthController();

        var result = controller.Get();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Ingest_InDevelopment_ReturnsIngestResponse()
    {
        var ingestion = Substitute.For<IDocumentIngestionService>();
        var expected = new IngestResponse(10, 10, "market-rag-index");
        ingestion.IngestSeedDataAsync(Arg.Any<CancellationToken>()).Returns(expected);
        var controller = new DevIngestionController(ingestion, TestWebHostEnvironment.Create("Development"));

        var actionResult = await controller.Ingest(CancellationToken.None);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Ingest_OutsideDevelopment_ReturnsNotFound()
    {
        var ingestion = Substitute.For<IDocumentIngestionService>();
        var controller = new DevIngestionController(ingestion, TestWebHostEnvironment.Create("Production"));

        var actionResult = await controller.Ingest(CancellationToken.None);

        actionResult.Result.Should().BeOfType<NotFoundResult>();
        await ingestion.DidNotReceive().IngestSeedDataAsync(Arg.Any<CancellationToken>());
    }
}
