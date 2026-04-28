namespace FoundryRag.Api.Services;

public interface IEmbeddingService
{
    Task<IReadOnlyList<float>> GenerateEmbeddingAsync(string input, CancellationToken cancellationToken);
}
