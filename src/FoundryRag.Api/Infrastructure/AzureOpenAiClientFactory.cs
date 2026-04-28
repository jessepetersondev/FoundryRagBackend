using Azure.AI.OpenAI;
using FoundryRag.Api.Options;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using OpenAI.Embeddings;
using System.ClientModel;

namespace FoundryRag.Api.Infrastructure;

public sealed class AzureOpenAiClientFactory
{
    private readonly AzureOpenAiOptions _options;
    private readonly Lazy<AzureOpenAIClient> _client;

    public AzureOpenAiClientFactory(IOptions<AzureOpenAiOptions> options)
    {
        _options = options.Value;
        _client = new Lazy<AzureOpenAIClient>(CreateClient);
    }

    public ChatClient GetChatClient() => _client.Value.GetChatClient(_options.ChatDeploymentName);

    public EmbeddingClient GetEmbeddingClient() => _client.Value.GetEmbeddingClient(_options.EmbeddingDeploymentName);

    private AzureOpenAIClient CreateClient()
    {
        if (!Uri.TryCreate(_options.Endpoint, UriKind.Absolute, out var endpoint))
        {
            throw new ConfigurationMissingException("Azure OpenAI endpoint is missing or invalid.");
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new ConfigurationMissingException("Azure OpenAI API key is missing.");
        }

        return new AzureOpenAIClient(endpoint, new ApiKeyCredential(_options.ApiKey));
    }
}
