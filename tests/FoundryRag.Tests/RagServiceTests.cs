using FluentAssertions;
using FoundryRag.Api.Contracts;
using FoundryRag.Api.Infrastructure;
using FoundryRag.Api.Models;
using FoundryRag.Api.Options;
using FoundryRag.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FoundryRag.Tests;

public sealed class RagServiceTests
{
    [Fact]
    public async Task AskAsync_EmptyQuestion_ThrowsValidationError()
    {
        var sut = CreateSut();

        var act = () => sut.AskAsync(new AskRequest("   ", null), CancellationToken.None);

        await act.Should().ThrowAsync<RequestValidationException>()
            .WithMessage("Question is required.");
    }

    [Fact]
    public async Task AskAsync_TopKAboveMax_ClampsBeforeSearch()
    {
        var vectorSearch = Substitute.For<IVectorSearchService>();
        vectorSearch.SearchAsync(Arg.Any<IReadOnlyList<float>>(), 10, Arg.Any<CancellationToken>())
            .Returns([CreateDocument()]);

        var sut = CreateSut(vectorSearch: vectorSearch);

        await sut.AskAsync(new AskRequest("Which markets mention inflation?", 99), CancellationToken.None);

        await vectorSearch.Received(1).SearchAsync(Arg.Any<IReadOnlyList<float>>(), 10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AskAsync_NoDocuments_ReturnsInsufficiencyAndDoesNotCallChat()
    {
        var vectorSearch = Substitute.For<IVectorSearchService>();
        vectorSearch.SearchAsync(Arg.Any<IReadOnlyList<float>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<RetrievedDocument>());
        var chat = Substitute.For<IChatCompletionService>();

        var sut = CreateSut(vectorSearch: vectorSearch, chat: chat);

        var response = await sut.AskAsync(new AskRequest("What about lunar mining?", 5), CancellationToken.None);

        response.Answer.Should().Be(RagService.InsufficientInformationAnswer);
        response.Sources.Should().BeEmpty();
        response.Retrieval.DocumentsReturned.Should().Be(0);
        await chat.DidNotReceive().GenerateAnswerAsync(Arg.Any<RagPrompt>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AskAsync_RetrievedDocuments_MapToSources()
    {
        var document = CreateDocument();
        var vectorSearch = Substitute.For<IVectorSearchService>();
        vectorSearch.SearchAsync(Arg.Any<IReadOnlyList<float>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([document]);

        var sut = CreateSut(vectorSearch: vectorSearch);

        var response = await sut.AskAsync(new AskRequest("Which markets mention inflation?", 5), CancellationToken.None);

        response.Answer.Should().Be("Grounded answer. [market-001]");
        response.Sources.Should().ContainSingle().Which.Should().BeEquivalentTo(
            new SourceReference(document.Id, document.Title, document.Category, document.Source, document.Score));
        response.Retrieval.Should().Be(new RetrievalMetadata(5, 1));
    }

    [Fact]
    public async Task AskAsync_AnswerWithUnknownCitation_ReturnsGuardedAnswer()
    {
        var chat = Substitute.For<IChatCompletionService>();
        chat.GenerateAnswerAsync(Arg.Any<RagPrompt>(), Arg.Any<CancellationToken>())
            .Returns("This mentions a source that was not retrieved. [market-999]");
        var sut = CreateSut(chat: chat);

        var response = await sut.AskAsync(new AskRequest("Which markets mention inflation?", 5), CancellationToken.None);

        response.Answer.Should().Contain(RagService.InsufficientInformationAnswer);
        response.Answer.Should().Contain("[market-001]");
        response.Sources.Should().ContainSingle(source => source.Id == "market-001");
    }

    [Fact]
    public async Task AskAsync_AnswerWithoutCitations_ReturnsGuardedAnswerWithRetrievedSource()
    {
        var chat = Substitute.For<IChatCompletionService>();
        chat.GenerateAnswerAsync(Arg.Any<RagPrompt>(), Arg.Any<CancellationToken>())
            .Returns("This answer describes the retrieved market but does not cite it.");
        var sut = CreateSut(chat: chat);

        var response = await sut.AskAsync(new AskRequest("Which markets mention inflation?", 5), CancellationToken.None);

        response.Answer.Should().Contain(RagService.InsufficientInformationAnswer);
        response.Answer.Should().Contain("[market-001]");
    }

    [Fact]
    public async Task AskAsync_WhitespaceChatAnswer_ReturnsInsufficiency()
    {
        var chat = Substitute.For<IChatCompletionService>();
        chat.GenerateAnswerAsync(Arg.Any<RagPrompt>(), Arg.Any<CancellationToken>())
            .Returns("   ");
        var sut = CreateSut(chat: chat);

        var response = await sut.AskAsync(new AskRequest("Which markets mention inflation?", 5), CancellationToken.None);

        response.Answer.Should().Be(RagService.InsufficientInformationAnswer);
    }

    private static RagService CreateSut(
        IEmbeddingService? embedding = null,
        IVectorSearchService? vectorSearch = null,
        IPromptBuilder? promptBuilder = null,
        IChatCompletionService? chat = null,
        IAnswerGroundingValidator? answerGroundingValidator = null)
    {
        embedding ??= Substitute.For<IEmbeddingService>();
        embedding.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([0.1f, 0.2f, 0.3f]);

        if (vectorSearch is null)
        {
            vectorSearch = Substitute.For<IVectorSearchService>();
            vectorSearch.SearchAsync(Arg.Any<IReadOnlyList<float>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns([CreateDocument()]);
        }

        promptBuilder ??= Substitute.For<IPromptBuilder>();
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
            Options.Create(new RagOptions { DefaultTopK = 5, MaxTopK = 10, MaxQuestionLength = 100 }),
            NullLogger<RagService>.Instance);
    }

    private static RetrievedDocument CreateDocument()
    {
        return new RetrievedDocument(
            "market-001",
            "CPI inflation sample market",
            "Economics",
            "Content about CPI and inflation.",
            "sample",
            0.89);
    }
}
