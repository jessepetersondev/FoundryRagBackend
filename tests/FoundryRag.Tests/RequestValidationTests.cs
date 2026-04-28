using FluentAssertions;
using FoundryRag.Api.Contracts;
using FoundryRag.Api.Infrastructure;

namespace FoundryRag.Tests;

public sealed class RequestValidationTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AskAsync_RejectsEmptyQuestions(string question)
    {
        var sut = TestRagServiceFactory.Create();

        var act = () => sut.AskAsync(new AskRequest(question, 5), CancellationToken.None);

        await act.Should().ThrowAsync<RequestValidationException>();
    }

    [Fact]
    public async Task AskAsync_RejectsOverlongQuestion()
    {
        var sut = TestRagServiceFactory.Create(maxQuestionLength: 10);

        var act = () => sut.AskAsync(new AskRequest("This question is much too long.", 5), CancellationToken.None);

        await act.Should().ThrowAsync<RequestValidationException>()
            .WithMessage("Question must be 10 characters or fewer.");
    }

    [Theory]
    [InlineData(null, 5)]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(99, 10)]
    public async Task AskAsync_BoundsTopK(int? requestedTopK, int expectedTopK)
    {
        var vectorSearch = TestRagServiceFactory.CreateVectorSearch();
        var sut = TestRagServiceFactory.Create(vectorSearch: vectorSearch);

        await sut.AskAsync(new AskRequest("Which markets mention CPI?", requestedTopK), CancellationToken.None);

        await vectorSearch.ReceivedSearchAsync(expectedTopK);
    }
}
