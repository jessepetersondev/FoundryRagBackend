# Quickstart: Backend RAG Assistant for Market Data

This quickstart validates the local backend RAG flow after Azure resources are
configured manually.

## Prerequisites

- .NET 8 SDK installed.
- Azure OpenAI resource available through Microsoft Foundry or Azure OpenAI.
- Chat deployment configured, for example `gpt-4o-mini` or a GPT-4-class deployment.
- Embedding deployment configured, for example `text-embedding-3-small` or an equivalent Azure OpenAI embedding deployment.
- Azure AI Search service with permissions to create/update an index.

## Configuration

Use user secrets, environment variables, or local development settings. Do not
commit real keys.

Required settings:

```json
{
  "AzureOpenAi": {
    "Endpoint": "https://YOUR-RESOURCE.openai.azure.com/",
    "ApiKey": "YOUR-LOCAL-SECRET",
    "ChatDeploymentName": "gpt-4o-mini",
    "EmbeddingDeploymentName": "text-embedding-3-small"
  },
  "AzureSearch": {
    "Endpoint": "https://YOUR-SEARCH.search.windows.net",
    "ApiKey": "YOUR-LOCAL-SECRET",
    "IndexName": "market-rag-index"
  },
  "Rag": {
    "DefaultTopK": 5,
    "MaxTopK": 10,
    "MinScoreThreshold": 0.7,
    "Temperature": 0.1,
    "MaxOutputTokens": 600,
    "MaxQuestionLength": 1000,
    "MaxContextCharactersPerDocument": 2000,
    "EmbeddingDimensions": 1536
  }
}
```

## Run Locally

From the repository root:

```bash
dotnet restore
dotnet run --project src/FoundryRag.Api --urls http://localhost:5000
```

## Health Check

```bash
curl http://localhost:5000/api/health
```

Expected response:

```json
{
  "status": "ok"
}
```

## Ingest Seed Data

The development ingestion endpoint reads `src/FoundryRag.Api/Data/seed-markets.json`,
generates embeddings, ensures the Azure AI Search index exists, and uploads
documents.

```bash
curl -X POST http://localhost:5000/api/dev/ingest
```

Expected response:

```json
{
  "documentsRead": 10,
  "documentsUploaded": 10,
  "indexName": "market-rag-index"
}
```

## Ask a Grounded Question

```bash
curl -X POST http://localhost:5000/api/ask \
  -H "Content-Type: application/json" \
  -d '{"question":"What markets involve CPI or inflation?","topK":5}'
```

Expected response shape:

```json
{
  "answer": "The indexed data contains a CPI inflation sample market. [market-001]",
  "sources": [
    {
      "id": "market-001",
      "title": "Will CPI exceed 3% in March?",
      "category": "Economics",
      "score": 0.87,
      "source": "seed"
    }
  ],
  "retrieval": {
    "topKRequested": 5,
    "documentsReturned": 2
  }
}
```

## Verify Insufficiency Behavior

Ask about a topic not present in the seed data:

```bash
curl -X POST http://localhost:5000/api/ask \
  -H "Content-Type: application/json" \
  -d '{"question":"Which markets cover lunar mining permits?","topK":5}'
```

Expected behavior:

- The answer states: "I do not have enough information in the indexed data."
- The response does not invent market data.
- Logs show query receipt, retrieval count, and top scores when available.

## Manual Integration Checklist

- Configure Azure OpenAI and Azure AI Search settings locally.
- Run the API with `dotnet run`.
- Call `POST /api/dev/ingest` and confirm documents upload.
- Call `POST /api/ask` with an in-domain question and confirm source citations.
- Call `POST /api/ask` with an unrelated question and confirm insufficiency.
- Confirm logs include pipeline steps but do not include secrets or full prompts.
