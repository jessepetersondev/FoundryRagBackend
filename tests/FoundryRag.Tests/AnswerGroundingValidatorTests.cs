using FluentAssertions;
using FoundryRag.Api.Models;
using FoundryRag.Api.Services;

namespace FoundryRag.Tests;

public sealed class AnswerGroundingValidatorTests
{
    [Fact]
    public void Validate_KnownBracketedSourceIds_IsValid()
    {
        var sut = new AnswerGroundingValidator();

        var result = sut.Validate(
            "The CPI market mentions inflation. [market-001]",
            [CreateDocument("market-001")]);

        result.IsValid.Should().BeTrue();
        result.HasCitations.Should().BeTrue();
        result.CitedSourceIds.Should().Contain("market-001");
        result.UnknownSourceIds.Should().BeEmpty();
    }

    [Fact]
    public void Validate_UnknownBracketedSourceIds_IsInvalid()
    {
        var sut = new AnswerGroundingValidator();

        var result = sut.Validate(
            "The CPI market mentions inflation. [market-999]",
            [CreateDocument("market-001")]);

        result.IsValid.Should().BeFalse();
        result.HasCitations.Should().BeTrue();
        result.UnknownSourceIds.Should().Contain("market-999");
    }

    [Fact]
    public void Validate_NoCitationsWithRetrievedDocuments_IsInvalid()
    {
        var sut = new AnswerGroundingValidator();

        var result = sut.Validate(
            "The CPI market mentions inflation.",
            [CreateDocument("market-001")]);

        result.IsValid.Should().BeFalse();
        result.HasCitations.Should().BeFalse();
    }

    private static RetrievedDocument CreateDocument(string id) =>
        new(id, "CPI market", "Economics", "Content", "sample", 0.91);
}
