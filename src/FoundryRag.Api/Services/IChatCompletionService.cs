using FoundryRag.Api.Models;

namespace FoundryRag.Api.Services;

public interface IChatCompletionService
{
    Task<string> GenerateAnswerAsync(RagPrompt prompt, CancellationToken cancellationToken);
}
