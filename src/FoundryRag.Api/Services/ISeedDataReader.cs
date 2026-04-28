using FoundryRag.Api.Models;

namespace FoundryRag.Api.Services;

public interface ISeedDataReader
{
    Task<IReadOnlyList<MarketDocument>> ReadSeedDataAsync(CancellationToken cancellationToken);
}
