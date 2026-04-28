using FoundryRag.Api.Contracts;

namespace FoundryRag.Api.Services;

public interface IRagService
{
    Task<AskResponse> AskAsync(AskRequest request, CancellationToken cancellationToken);
}
