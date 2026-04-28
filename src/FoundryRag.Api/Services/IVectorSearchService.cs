using FoundryRag.Api.Models;

namespace FoundryRag.Api.Services;

public interface IVectorSearchService
{
    Task<IReadOnlyList<RetrievedDocument>> SearchAsync(IReadOnlyList<float> queryEmbedding, int topK, CancellationToken cancellationToken);

    Task EnsureIndexCreatedAsync(CancellationToken cancellationToken);

    Task UploadDocumentsAsync(IReadOnlyList<IndexedDocument> documents, CancellationToken cancellationToken);
}
