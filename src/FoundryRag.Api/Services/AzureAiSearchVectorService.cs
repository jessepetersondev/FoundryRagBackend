using System.Globalization;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using FoundryRag.Api.Infrastructure;
using FoundryRag.Api.Models;
using FoundryRag.Api.Options;
using Microsoft.Extensions.Options;

namespace FoundryRag.Api.Services;

public sealed class AzureAiSearchVectorService : IVectorSearchService
{
    private const string VectorProfileName = "embedding-profile";
    private const string HnswConfigName = "hnsw-config";

    private readonly AzureSearchClientFactory _clientFactory;
    private readonly RetryPolicy _retryPolicy;
    private readonly RagOptions _ragOptions;
    private readonly ILogger<AzureAiSearchVectorService> _logger;

    public AzureAiSearchVectorService(
        AzureSearchClientFactory clientFactory,
        RetryPolicy retryPolicy,
        IOptions<RagOptions> ragOptions,
        ILogger<AzureAiSearchVectorService> logger)
    {
        _clientFactory = clientFactory;
        _retryPolicy = retryPolicy;
        _ragOptions = ragOptions.Value;
        _logger = logger;
    }

    public async Task EnsureIndexCreatedAsync(CancellationToken cancellationToken)
    {
        await _retryPolicy.ExecuteAsync(async ct =>
        {
            var fields = new List<SearchField>
            {
                new SimpleField("id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true },
                new SearchableField("title") { IsFilterable = true, IsSortable = true },
                new SearchableField("category") { IsFilterable = true, IsFacetable = true },
                new SearchableField("content"),
                new SimpleField("source", SearchFieldDataType.String) { IsFilterable = true },
                new SimpleField("effectiveDate", SearchFieldDataType.String) { IsFilterable = true, IsSortable = true },
                new VectorSearchField("embedding", _ragOptions.EmbeddingDimensions, VectorProfileName)
            };

            var vectorSearch = new VectorSearch();
            vectorSearch.Algorithms.Add(new HnswAlgorithmConfiguration(HnswConfigName)
            {
                Parameters = new HnswParameters { Metric = VectorSearchAlgorithmMetric.Cosine }
            });
            vectorSearch.Profiles.Add(new VectorSearchProfile(VectorProfileName, HnswConfigName));

            var index = new SearchIndex(_clientFactory.IndexName, fields)
            {
                VectorSearch = vectorSearch
            };

            await _clientFactory.GetIndexClient().CreateOrUpdateIndexAsync(index, allowIndexDowntime: false, cancellationToken: ct);
            _logger.LogInformation("Ensured Azure AI Search index {IndexName}", _clientFactory.IndexName);
        }, "Azure AI Search index creation", cancellationToken);
    }

    public async Task UploadDocumentsAsync(IReadOnlyList<IndexedDocument> documents, CancellationToken cancellationToken)
    {
        if (documents.Count == 0)
        {
            return;
        }

        foreach (var document in documents)
        {
            ValidateEmbeddingDimensions(document.Embedding.Count, $"Document '{document.Id}' embedding");
        }

        await _retryPolicy.ExecuteAsync(async ct =>
        {
            var searchDocuments = documents.Select(ToSearchDocument).ToArray();
            await _clientFactory.GetSearchClient().MergeOrUploadDocumentsAsync(searchDocuments, cancellationToken: ct);
            _logger.LogInformation("Uploaded {DocumentCount} documents to index {IndexName}", searchDocuments.Length, _clientFactory.IndexName);
        }, "Azure AI Search document upload", cancellationToken);
    }

    public async Task<IReadOnlyList<RetrievedDocument>> SearchAsync(
        IReadOnlyList<float> queryEmbedding,
        int topK,
        CancellationToken cancellationToken)
    {
        if (queryEmbedding.Count == 0)
        {
            throw new RequestValidationException("Query embedding is required.");
        }

        ValidateEmbeddingDimensions(queryEmbedding.Count, "Query embedding");

        return await _retryPolicy.ExecuteAsync(async ct =>
        {
            var vectorQuery = new VectorizedQuery(queryEmbedding.ToArray())
            {
                KNearestNeighborsCount = topK
            };
            vectorQuery.Fields.Add("embedding");

            var options = new SearchOptions
            {
                Size = topK,
                VectorSearch = new VectorSearchOptions()
            };
            options.VectorSearch.Queries.Add(vectorQuery);
            options.Select.Add("id");
            options.Select.Add("title");
            options.Select.Add("category");
            options.Select.Add("content");
            options.Select.Add("source");
            options.Select.Add("effectiveDate");

            var response = await _clientFactory.GetSearchClient().SearchAsync<SearchDocument>("*", options, ct);
            var documents = new List<RetrievedDocument>();

            await foreach (var result in response.Value.GetResultsAsync().WithCancellation(ct))
            {
                if (!MeetsMinScoreThreshold(result.Score, _ragOptions.MinScoreThreshold))
                {
                    continue;
                }

                documents.Add(new RetrievedDocument(
                    GetString(result.Document, "id"),
                    GetString(result.Document, "title"),
                    GetString(result.Document, "category"),
                    GetString(result.Document, "content"),
                    GetString(result.Document, "source"),
                    result.Score));
            }

            _logger.LogInformation("Search returned {DocumentCount} documents after threshold filtering", documents.Count);
            return documents;
        }, "Azure AI Search vector query", cancellationToken);
    }

    private static SearchDocument ToSearchDocument(IndexedDocument document)
    {
        var searchDocument = new SearchDocument
        {
            ["id"] = document.Id,
            ["title"] = document.Title,
            ["category"] = document.Category,
            ["content"] = document.Content,
            ["source"] = document.Source,
            ["effectiveDate"] = document.EffectiveDate ?? string.Empty,
            ["embedding"] = document.Embedding.ToArray()
        };

        return searchDocument;
    }

    private static string GetString(SearchDocument document, string name)
    {
        return document.TryGetValue(name, out var value)
            ? Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
            : string.Empty;
    }

    internal static bool MeetsMinScoreThreshold(double? score, double minScoreThreshold)
    {
        if (minScoreThreshold <= 0)
        {
            return true;
        }

        return score is not null && score >= minScoreThreshold;
    }

    private void ValidateEmbeddingDimensions(int actualDimensions, string embeddingLabel)
    {
        if (actualDimensions != _ragOptions.EmbeddingDimensions)
        {
            throw new ConfigurationMissingException(
                $"{embeddingLabel} dimension count does not match configured Rag:EmbeddingDimensions.");
        }
    }
}
