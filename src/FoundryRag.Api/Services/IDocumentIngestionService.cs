using FoundryRag.Api.Contracts;

namespace FoundryRag.Api.Services;

public interface IDocumentIngestionService
{
    Task<IngestResponse> IngestSeedDataAsync(CancellationToken cancellationToken);
}
