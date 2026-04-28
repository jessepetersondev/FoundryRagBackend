using FluentAssertions;
using FoundryRag.Api.Contracts;
using FoundryRag.Api.Controllers;
using FoundryRag.Api.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace FoundryRag.Tests;

public sealed class AskControllerContractTests
{
    [Fact]
    public async Task Ask_ReturnsAnswerSourcesAndRetrievalMetadata()
    {
        var ragService = Substitute.For<IRagService>();
        var expected = new AskResponse(
            "Grounded answer.",
            [new SourceReference("market-001", "CPI market", "Economics", "sample", 0.87)],
            new RetrievalMetadata(5, 1));
        ragService.AskAsync(Arg.Any<AskRequest>(), Arg.Any<CancellationToken>()).Returns(expected);
        var controller = new AskController(ragService);

        var actionResult = await controller.Ask(new AskRequest("Which markets mention CPI?", 5), CancellationToken.None);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
    }
}
