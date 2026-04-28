# FoundryRagBackend

Backend RAG demo for answering questions about a small, fictional
Kalshi-style market dataset using Azure OpenAI and Azure AI Search.

The API validates a question, generates an Azure OpenAI embedding, retrieves
matching documents from Azure AI Search, builds a grounded prompt, calls the
configured chat deployment, validates source citations, and returns the answer
with retrieval metadata.

## Run Locally

Configure Azure OpenAI and Azure AI Search values with user secrets:

```bash
dotnet user-secrets init --project src/FoundryRag.Api
dotnet user-secrets set "AzureOpenAi:Endpoint" "https://YOUR-AZURE-OPENAI-RESOURCE.openai.azure.com/" --project src/FoundryRag.Api
dotnet user-secrets set "AzureOpenAi:ApiKey" "YOUR-KEY" --project src/FoundryRag.Api
dotnet user-secrets set "AzureOpenAi:ChatDeploymentName" "gpt-4o-mini" --project src/FoundryRag.Api
dotnet user-secrets set "AzureOpenAi:EmbeddingDeploymentName" "text-embedding-3-small" --project src/FoundryRag.Api
dotnet user-secrets set "AzureSearch:Endpoint" "https://YOUR-SEARCH-SERVICE.search.windows.net" --project src/FoundryRag.Api
dotnet user-secrets set "AzureSearch:ApiKey" "YOUR-SEARCH-ADMIN-KEY" --project src/FoundryRag.Api
dotnet user-secrets set "AzureSearch:IndexName" "market-rag-index" --project src/FoundryRag.Api
dotnet user-secrets set "Rag:EmbeddingDimensions" "1536" --project src/FoundryRag.Api
```

Build, test, and run:

```bash
dotnet restore
dotnet build FoundryRagBackend.sln
dotnet test FoundryRagBackend.sln
dotnet run --project src/FoundryRag.Api --urls http://localhost:5000
```

Open the simple frontend:

```text
http://localhost:5000/
```

Ingest the seed data:

```bash
curl -X POST http://localhost:5000/api/dev/ingest
```

Ask a question:

```bash
curl -X POST http://localhost:5000/api/ask \
  -H "Content-Type: application/json" \
  -d '{"question":"What markets involve CPI or inflation?","topK":5}'
```

## Documentation

Detailed API setup, architecture, troubleshooting, and Azure verification notes
are in [src/FoundryRag.Api/README.md](src/FoundryRag.Api/README.md).

Do not commit real Azure keys. The checked-in appsettings files are placeholders;
use user secrets, environment variables, or a local untracked settings file for
real credentials.
