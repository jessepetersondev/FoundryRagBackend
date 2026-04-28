# FoundryRagBackend

Backend RAG demo for answering questions about a small, fictional
Kalshi-style market dataset using Azure OpenAI and Azure AI Search.

## Architecture

```text
User -> API -> Embedding -> Azure AI Search -> Prompt Builder -> Azure OpenAI -> Response
```

The API is intentionally backend-only. Controllers handle HTTP concerns and call
application interfaces. Azure SDK clients are isolated behind services and
factories so the RAG workflow can be unit tested without live Azure resources.

## RAG Flow

1. `POST /api/ask` receives a question and optional `topK`.
2. `RagService` validates the question and clamps `topK`.
3. `IEmbeddingService` generates a query embedding with Azure OpenAI.
4. `IVectorSearchService` retrieves top-k documents from Azure AI Search.
5. If retrieval returns no usable documents, the API returns:
   `I do not have enough information in the indexed data to answer that.`
6. `GroundedPromptBuilder` builds a prompt with delimited source documents.
7. `IChatCompletionService` calls the configured Azure OpenAI chat deployment.
8. The API returns answer text, source references, and retrieval metadata.

The prompt instructs the model to use only retrieved context, treat context as
data rather than instructions, cite source IDs or titles, and avoid investment
advice.

## Azure Resources

Create these resources manually for local development:

- Azure OpenAI resource available through Microsoft Foundry or Azure OpenAI.
- Chat deployment, for example `gpt-4o-mini`.
- Embedding deployment, for example `text-embedding-3-small`.
- Azure AI Search service with permission to create/update an index.

The configured `Rag:EmbeddingDimensions` must match the deployed embedding model.
The default `1536` matches common `text-embedding-3-small` deployments.

## Configuration

Use user secrets, environment variables, or a local untracked settings file. Do
not commit real keys.

```json
{
  "AzureOpenAi": {
    "Endpoint": "https://YOUR-AZURE-OPENAI-RESOURCE.openai.azure.com/",
    "ApiKey": "",
    "ChatDeploymentName": "gpt-4o-mini",
    "EmbeddingDeploymentName": "text-embedding-3-small"
  },
  "AzureSearch": {
    "Endpoint": "https://YOUR-SEARCH-SERVICE.search.windows.net",
    "ApiKey": "",
    "IndexName": "market-rag-index"
  },
    "Rag": {
      "DefaultTopK": 5,
      "MaxTopK": 10,
      "MinScoreThreshold": 0.62,
    "Temperature": 0.1,
    "MaxOutputTokens": 800,
    "MaxQuestionLength": 1000,
    "MaxContextCharactersPerDocument": 1800,
    "EmbeddingDimensions": 1536
  }
}
```

Example user-secrets setup:

```bash
dotnet user-secrets init --project src/FoundryRag.Api
dotnet user-secrets set "AzureOpenAi:Endpoint" "https://YOUR-AZURE-OPENAI-RESOURCE.openai.azure.com/" --project src/FoundryRag.Api
dotnet user-secrets set "AzureOpenAi:ApiKey" "YOUR-KEY" --project src/FoundryRag.Api
dotnet user-secrets set "AzureOpenAi:ChatDeploymentName" "gpt-4o-mini" --project src/FoundryRag.Api
dotnet user-secrets set "AzureOpenAi:EmbeddingDeploymentName" "text-embedding-3-small" --project src/FoundryRag.Api
dotnet user-secrets set "AzureSearch:Endpoint" "https://YOUR-SEARCH-SERVICE.search.windows.net" --project src/FoundryRag.Api
dotnet user-secrets set "AzureSearch:ApiKey" "YOUR-KEY" --project src/FoundryRag.Api
dotnet user-secrets set "AzureSearch:IndexName" "market-rag-index" --project src/FoundryRag.Api
dotnet user-secrets set "Rag:EmbeddingDimensions" "1536" --project src/FoundryRag.Api
```

## Local Setup

```bash
dotnet restore
dotnet build FoundryRagBackend.sln
dotnet test FoundryRagBackend.sln
dotnet run --project src/FoundryRag.Api
```

The app validates required Azure settings on startup.

For live Azure validation, follow the dedicated manual guide:
[docs/manual-azure-verification.md](../../docs/manual-azure-verification.md).

## Frontend

The API serves a simple same-origin frontend from `wwwroot`.

```bash
dotnet run --project src/FoundryRag.Api --urls http://localhost:5000
```

Open:

```text
http://localhost:5000/
```

## Health

```bash
curl http://localhost:5000/api/health
```

Expected:

```json
{
  "status": "ok"
}
```

## Ingest Seed Data

The development endpoint reads `Data/seed-markets.json`, creates/updates the
Azure AI Search index, generates embeddings, and uploads documents.

```bash
curl -X POST http://localhost:5000/api/dev/ingest
```

Expected:

```json
{
  "documentsRead": 10,
  "documentsUploaded": 10,
  "indexName": "market-rag-index"
}
```

## Ask

```bash
curl -X POST http://localhost:5000/api/ask \
  -H "Content-Type: application/json" \
  -d '{"question":"What markets involve CPI or inflation?","topK":5}'
```

Expected shape:

```json
{
  "answer": "The indexed data contains a CPI inflation sample market. [market-001]",
  "sources": [
    {
      "id": "market-001",
      "title": "Will CPI exceed 3% in March?",
      "category": "Economics",
      "source": "sample-seed-data",
      "score": 0.87
    }
  ],
  "retrieval": {
    "topKRequested": 5,
    "documentsReturned": 1
  }
}
```

## Example Questions

- What markets involve CPI or inflation?
- Which sample markets are in the Economics category?
- Are there any weather-related markets?
- Which markets mention elections or polling?

For unrelated questions, the service should return the insufficiency response
instead of inventing market data.

## Troubleshooting

- Startup fails with configuration errors: set required Azure settings using
  user secrets or environment variables.
- Ingestion fails: verify the Azure AI Search endpoint/key and index permissions.
- Ask fails after ingestion: verify chat and embedding deployment names.
- Retrieval returns no documents: check `Rag:MinScoreThreshold`, embedding
  dimensions, and whether ingestion completed.

## Tradeoffs

- Uses simple document-level indexing for an interview-friendly demo.
- Includes a minimal static frontend for local testing, but does not include
  authentication, production deployment, CI/CD, advanced reranking, streaming
  responses, or complex chunking.
- `Rag:Temperature` is configurable and applied when the installed OpenAI chat
  SDK exposes a writable temperature option. The restored Azure OpenAI SDK path
  used by this project exposes max output tokens but does not expose temperature
  on `ChatCompletionOptions`.

## Known Limitations

- Live Azure verification must be run manually with real Azure resources.
- Prompt grounding is strengthened with bracketed source citation validation, but
  it is not a full semantic proof of every generated claim.
- The local demo does not include authentication.
- Production deployment, CI/CD, Application Insights, and managed identity are
  intentionally out of scope for this version.

## Remediations After Analysis

- Added post-generation validation for bracketed source IDs.
- Strengthened the prompt to require bracketed source IDs such as `[market-001]`.
- Sanitized seed-data file loading errors so local filesystem paths are not
  returned to API clients.
- Added embedding dimension checks before search and ingestion upload.
- Tightened score-threshold behavior for null Azure Search scores.
- Added manual Azure verification guidance for completing `T048`.

## Interview Explanation

"This project demonstrates a backend RAG system using Azure OpenAI and vector
search. It embeds documents, stores them in Azure AI Search, retrieves top-k
relevant context for a user query, and sends only that context to the LLM for
grounded answers."
