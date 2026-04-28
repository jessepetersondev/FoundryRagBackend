using FoundryRag.Api.Infrastructure;
using Microsoft.Extensions.Options;
using FoundryRag.Api.Options;

namespace FoundryRag.Api.Services;

public sealed class AzureOpenAiEmbeddingService : IEmbeddingService
{
    private readonly AzureOpenAiClientFactory _clientFactory;
    private readonly RetryPolicy _retryPolicy;
    private readonly AzureOpenAiOptions _options;
    private readonly ILogger<AzureOpenAiEmbeddingService> _logger;

    public AzureOpenAiEmbeddingService(
        AzureOpenAiClientFactory clientFactory,
        RetryPolicy retryPolicy,
        IOptions<AzureOpenAiOptions> options,
        ILogger<AzureOpenAiEmbeddingService> logger)
    {
        _clientFactory = clientFactory;
        _retryPolicy = retryPolicy;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<float>> GenerateEmbeddingAsync(string input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new RequestValidationException("Embedding input is required.");
        }

        return await _retryPolicy.ExecuteAsync(async ct =>
        {
            var client = _clientFactory.GetEmbeddingClient();
            var result = await client.GenerateEmbeddingAsync(input, options: null, cancellationToken: ct);
            var embedding = result.Value.ToFloats().ToArray();
            _logger.LogInformation("Generated embedding with {DimensionCount} dimensions using deployment {DeploymentName}", embedding.Length, _options.EmbeddingDeploymentName);
            return embedding;
        }, "Azure OpenAI embedding generation", cancellationToken);
    }
}
