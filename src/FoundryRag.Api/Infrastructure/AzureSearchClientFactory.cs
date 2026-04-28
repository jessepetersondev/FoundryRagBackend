using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using FoundryRag.Api.Options;
using Microsoft.Extensions.Options;

namespace FoundryRag.Api.Infrastructure;

public sealed class AzureSearchClientFactory
{
    private readonly AzureSearchOptions _options;
    private readonly Lazy<SearchIndexClient> _indexClient;
    private readonly Lazy<SearchClient> _searchClient;

    public AzureSearchClientFactory(IOptions<AzureSearchOptions> options)
    {
        _options = options.Value;
        _indexClient = new Lazy<SearchIndexClient>(CreateIndexClient);
        _searchClient = new Lazy<SearchClient>(CreateSearchClient);
    }

    public string IndexName => _options.IndexName;

    public SearchIndexClient GetIndexClient() => _indexClient.Value;

    public SearchClient GetSearchClient() => _searchClient.Value;

    private SearchIndexClient CreateIndexClient()
    {
        if (!Uri.TryCreate(_options.Endpoint, UriKind.Absolute, out var endpoint))
        {
            throw new ConfigurationMissingException("Azure AI Search endpoint is missing or invalid.");
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new ConfigurationMissingException("Azure AI Search API key is missing.");
        }

        return new SearchIndexClient(endpoint, new AzureKeyCredential(_options.ApiKey));
    }

    private SearchClient CreateSearchClient() => GetIndexClient().GetSearchClient(_options.IndexName);
}
