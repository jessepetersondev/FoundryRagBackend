# FoundryRagBackend

FoundryRagBackend is a local .NET 8 RAG application that answers questions about
a small fictional market dataset. It uses Azure OpenAI for embeddings and chat,
Azure AI Search for vector retrieval, and a simple static frontend served by the
API.

The point of the project is to show the full backend RAG loop:

```text
User question
  -> ASP.NET Core API
  -> Azure OpenAI embedding
  -> Azure AI Search vector retrieval
  -> grounded prompt with retrieved market context
  -> Azure OpenAI chat completion
  -> answer with source citations and retrieval metadata
```

## What It Does

- Ingests a seed dataset of fictional market/event documents.
- Creates or updates an Azure AI Search vector index.
- Generates embeddings for each seed document with Azure OpenAI.
- Accepts user questions through `POST /api/ask` or the browser UI.
- Retrieves relevant documents before calling the chat model.
- Forces answers to use indexed context and cite source IDs such as
  `[market-001]`.
- Returns a safe insufficiency response when the indexed data does not support
  an answer.

This is intentionally a local demo, not a production deployment template.

## Architecture

```text
src/FoundryRag.Api
|-- Controllers/      HTTP endpoints only
|-- Contracts/        Request and response DTOs
|-- Data/             Seed market dataset
|-- Infrastructure/   Azure client factories, retry, error handling
|-- Models/           Domain and retrieval models
|-- Options/          Strongly typed configuration
|-- Services/         RAG workflow, ingestion, prompt, grounding, Azure adapters
`-- wwwroot/          Simple frontend UI

tests/FoundryRag.Tests
`-- Unit and contract-style tests using fakes instead of live Azure services
```

The controllers do not call Azure SDKs directly. Azure clients stay behind
application-owned services and interfaces so the RAG workflow is testable
without live Azure resources.

## RAG Flow

### Ingestion Flow

`POST /api/dev/ingest` runs only in Development mode.

1. Reads `src/FoundryRag.Api/Data/seed-markets.json`.
2. Converts each market record into searchable text.
3. Generates one embedding per document using the configured Azure OpenAI
   embedding deployment.
4. Creates or updates `market-rag-index` in Azure AI Search.
5. Uploads the indexed documents, including vector embeddings and source
   metadata.

Expected ingestion response:

```json
{
  "documentsRead": 10,
  "documentsUploaded": 10,
  "indexName": "market-rag-index"
}
```

### Question Flow

`POST /api/ask` handles RAG questions.

1. Validates the question and clamps `topK`.
2. Generates an embedding for the user question.
3. Runs a vector query against Azure AI Search.
4. Filters weak matches using `Rag:MinScoreThreshold`.
5. Builds a prompt containing only the retrieved documents.
6. Calls the Azure OpenAI chat deployment.
7. Validates that the answer cites retrieved source IDs.
8. Returns the answer, source references, and retrieval metadata.

If no useful context is retrieved, or if the generated answer is not safely
grounded, the API returns:

```text
I do not have enough information in the indexed data to answer that.
```

## Azure Resources

You need two Azure services.

### Azure OpenAI

Create or use an Azure OpenAI resource through Azure AI Foundry or the Azure
portal.

Record these values:

- Endpoint: `https://<openai-resource-name>.openai.azure.com/`
- API key
- Chat deployment name, for example `gpt-4o-mini`
- Embedding deployment name, for example `text-embedding-3-small`

Use the deployment names, not just the model names. The code calls deployments.

### Azure AI Search

Create or use an Azure AI Search service.

Record these values:

- Endpoint: `https://<search-service-name>.search.windows.net`
- Admin API key
- Index name, usually `market-rag-index`

Use an admin key for local ingestion because the app creates or updates the
index. A query key is not enough.

## Configuration

Do not put real Azure keys in committed files. The checked-in appsettings files
are placeholders. Use ASP.NET user secrets for local development:

```bash
dotnet user-secrets init --project src/FoundryRag.Api

dotnet user-secrets set "AzureOpenAi:Endpoint" "https://YOUR-AZURE-OPENAI-RESOURCE.openai.azure.com/" --project src/FoundryRag.Api
dotnet user-secrets set "AzureOpenAi:ApiKey" "YOUR-AZURE-OPENAI-KEY" --project src/FoundryRag.Api
dotnet user-secrets set "AzureOpenAi:ChatDeploymentName" "gpt-4o-mini" --project src/FoundryRag.Api
dotnet user-secrets set "AzureOpenAi:EmbeddingDeploymentName" "text-embedding-3-small" --project src/FoundryRag.Api

dotnet user-secrets set "AzureSearch:Endpoint" "https://YOUR-SEARCH-SERVICE.search.windows.net" --project src/FoundryRag.Api
dotnet user-secrets set "AzureSearch:ApiKey" "YOUR-SEARCH-ADMIN-KEY" --project src/FoundryRag.Api
dotnet user-secrets set "AzureSearch:IndexName" "market-rag-index" --project src/FoundryRag.Api

dotnet user-secrets set "Rag:EmbeddingDimensions" "1536" --project src/FoundryRag.Api
```

The default `Rag:EmbeddingDimensions` value of `1536` matches
`text-embedding-3-small`. If you deploy a different embedding model or configure
custom dimensions, update this value to match.

Current RAG defaults:

```json
{
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

## Run It

From the repository root:

```bash
dotnet restore
dotnet build FoundryRagBackend.sln
dotnet test FoundryRagBackend.sln
dotnet run --project src/FoundryRag.Api --urls http://localhost:5000
```

Open the frontend:

```text
http://localhost:5000/
```

Health check:

```bash
curl http://localhost:5000/api/health
```

Expected:

```json
{
  "status": "ok"
}
```

## Seed The Index

Run this after the API starts:

```bash
curl -X POST http://localhost:5000/api/dev/ingest
```

The app will create or update the Azure AI Search index and upload the seed
documents. After this succeeds, you can inspect the index in the Azure portal
under your Azure AI Search service:

```text
Search management -> Indexes -> market-rag-index -> Search explorer
```

Example Search explorer query:

```json
{
  "search": "*",
  "count": true,
  "select": "id,title,category,source,effectiveDate",
  "top": 10
}
```

## Ask Questions

Using the UI:

```text
http://localhost:5000/
```

Using curl:

```bash
curl -X POST http://localhost:5000/api/ask \
  -H "Content-Type: application/json" \
  -d '{"question":"What markets involve CPI or inflation?","topK":5}'
```

Example response shape:

```json
{
  "answer": "The indexed data contains a CPI market about whether the March CPI year-over-year reading exceeds 3%. [market-001]",
  "sources": [
    {
      "id": "market-001",
      "title": "Will CPI exceed 3% in March?",
      "category": "Economics",
      "source": "sample-seed-data",
      "score": 0.69
    }
  ],
  "retrieval": {
    "topKRequested": 5,
    "documentsReturned": 1
  }
}
```

Good demo questions:

- What markets involve CPI or inflation?
- Which markets mention sports championships?
- Which sample markets are in the Economics category?
- Are there any weather-related markets?
- Which markets mention elections or polling?
- Which markets cover lunar mining permits?

The lunar mining question should return an insufficiency response because that
topic is not in the indexed seed data.

## API Endpoints

| Method | Path | Purpose |
| --- | --- | --- |
| `GET` | `/` | Static frontend |
| `GET` | `/api/health` | Health check |
| `POST` | `/api/dev/ingest` | Development-only seed ingestion |
| `POST` | `/api/ask` | RAG question answering |

Ask request:

```json
{
  "question": "What markets involve CPI or inflation?",
  "topK": 5
}
```

Ask response:

```json
{
  "answer": "string",
  "sources": [
    {
      "id": "market-001",
      "title": "Will CPI exceed 3% in March?",
      "category": "Economics",
      "source": "sample-seed-data",
      "score": 0.69
    }
  ],
  "retrieval": {
    "topKRequested": 5,
    "documentsReturned": 1
  }
}
```

## Grounding And Safety

The project includes several guardrails:

- Retrieved documents are delimited in the prompt.
- The prompt tells the model to use retrieved context as data, not instructions.
- The prompt blocks investment advice and unsupported claims.
- Answers must cite retrieved source IDs in bracket form, for example
  `[market-001]`.
- The service validates citations after generation.
- If citations are missing or invalid, the API returns a safe cited
  insufficiency answer instead of trusting the model output.
- Controllers only handle HTTP; business logic and Azure clients are behind
  testable services.

This is not a formal proof that every sentence is correct, but it makes the
answer path explicit and observable.

## Tests

Run all tests:

```bash
dotnet test FoundryRagBackend.sln
```

The tests cover:

- request validation
- top-k clamping
- prompt construction
- prompt-injection handling
- citation validation
- insufficient-context responses
- Azure AI Search vector service behavior
- Azure OpenAI chat option construction
- seed ingestion
- error handling
- controller contracts

Live Azure calls are not required for unit tests. Manual live checks are
documented in [docs/manual-azure-verification.md](docs/manual-azure-verification.md).

## Troubleshooting

### Startup Fails With Configuration Errors

The app validates required settings on startup. Confirm user secrets are set for:

- `AzureOpenAi:Endpoint`
- `AzureOpenAi:ApiKey`
- `AzureOpenAi:ChatDeploymentName`
- `AzureOpenAi:EmbeddingDeploymentName`
- `AzureSearch:Endpoint`
- `AzureSearch:ApiKey`
- `AzureSearch:IndexName`

### Ingestion Fails

Check:

- Azure AI Search endpoint is correct.
- Search key is an admin key.
- The Search service allows index creation.
- `Rag:EmbeddingDimensions` matches the embedding deployment.
- The Azure OpenAI embedding deployment name is correct.

### Questions Return No Sources

Check:

- Seed ingestion completed successfully.
- `market-rag-index` contains documents in Azure Search.
- `Rag:MinScoreThreshold` is not too high.
- The question is actually related to the seed dataset.

### Outcome Questions Say The Result Is Missing

The seed data defines sample markets and resolution rules. It does not contain
live market prices, probabilities, or final outcomes. For example, if you ask
whether CPI actually exceeded 3%, the correct answer is that the indexed data
does not contain the result.

## Tradeoffs

- Document-level vectors are used instead of chunking because the seed records
  are short.
- The frontend is intentionally simple and same-origin, avoiding CORS and build
  tooling.
- Authentication, production deployment, managed identity, streaming responses,
  reranking, Application Insights, and CI/CD are out of scope.
- `Rag:Temperature` is configurable, but the installed Azure OpenAI SDK path may
  not expose a writable temperature option for every package version.

## Interview Summary

I built a backend RAG system using Azure OpenAI and Azure AI Search. It ingests
domain documents, embeds them, stores vectors in Azure AI Search, retrieves
relevant context for each user question, sends only that context to the chat
model, and returns grounded answers with source citations.
