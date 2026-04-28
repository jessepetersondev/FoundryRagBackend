using FoundryRag.Api.Models;

namespace FoundryRag.Api.Services;

public interface IPromptBuilder
{
    RagPrompt BuildPrompt(string question, IReadOnlyList<RetrievedDocument> documents);
}
