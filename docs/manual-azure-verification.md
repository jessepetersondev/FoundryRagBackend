# Manual Azure Verification

Use this checklist to complete `T048` after real Azure resources and local
configuration are available. Do not mark `T048` complete until these calls have
been run against live Azure OpenAI and Azure AI Search resources.

## Current Status: Blocked

Last checked: 2026-04-27.

Live Azure verification is currently blocked because required live configuration
is not present in this workspace:

- `AzureOpenAi:Endpoint` is still a placeholder.
- `AzureOpenAi:ApiKey` is not configured.
- `AzureSearch:Endpoint` is still a placeholder.
- `AzureSearch:ApiKey` is not configured.
- The API project does not currently contain a `UserSecretsId`, so user secrets
  have not been initialized for `src/FoundryRag.Api`.

Local pre-flight passed on 2026-04-27:

- `dotnet restore FoundryRagBackend.sln`
- `dotnet build FoundryRagBackend.sln`
- `dotnet test FoundryRagBackend.sln` with 47/47 tests passing

`T048` must remain pending until the live Azure resources and local secrets are
configured and the ingestion plus ask flows below are executed successfully.

## 1. Azure OpenAI Deployments

Create or identify an Azure OpenAI resource available through Microsoft Foundry
or Azure OpenAI. Confirm these deployments exist:

- Chat deployment, for example `gpt-4o-mini` or a GPT-4-class deployment.
- Embedding deployment, for example `text-embedding-3-small`.

Record:

- Azure OpenAI endpoint: `https://<resource-name>.openai.azure.com/`
- Azure OpenAI API key
- Chat deployment name
- Embedding deployment name
- Embedding dimensions for the deployed embedding model

## 2. Azure AI Search Service

Create or identify an Azure AI Search service with permission to create and
update indexes.

Record:

- Search endpoint: `https://<search-service-name>.search.windows.net`
- Search admin API key
- Index name, for example `market-rag-index`

The API creates or updates the vector index during ingestion.

## 3. Local Configuration

From the repository root:

```bash
dotnet user-secrets init --project src/FoundryRag.Api
dotnet user-secrets set "AzureOpenAi:Endpoint" "https://<resource-name>.openai.azure.com/" --project src/FoundryRag.Api
dotnet user-secrets set "AzureOpenAi:ApiKey" "<azure-openai-key>" --project src/FoundryRag.Api
dotnet user-secrets set "AzureOpenAi:ChatDeploymentName" "<chat-deployment-name>" --project src/FoundryRag.Api
dotnet user-secrets set "AzureOpenAi:EmbeddingDeploymentName" "<embedding-deployment-name>" --project src/FoundryRag.Api
dotnet user-secrets set "AzureSearch:Endpoint" "https://<search-service-name>.search.windows.net" --project src/FoundryRag.Api
dotnet user-secrets set "AzureSearch:ApiKey" "<azure-search-admin-key>" --project src/FoundryRag.Api
dotnet user-secrets set "AzureSearch:IndexName" "market-rag-index" --project src/FoundryRag.Api
dotnet user-secrets set "Rag:EmbeddingDimensions" "1536" --project src/FoundryRag.Api
```

Change `Rag:EmbeddingDimensions` if the embedding deployment uses a different
dimension count.

## 4. Run the API

```bash
dotnet restore
dotnet build FoundryRagBackend.sln
dotnet run --project src/FoundryRag.Api --urls http://localhost:5000
```

In another terminal, confirm health:

```bash
curl http://localhost:5000/api/health
```

Expected:

```json
{
  "status": "ok"
}
```

## 5. Ingest Seed Data

```bash
curl -X POST http://localhost:5000/api/dev/ingest
```

Expected shape:

```json
{
  "documentsRead": 18,
  "documentsUploaded": 18,
  "indexName": "market-rag-index"
}
```

Confirm in Azure AI Search that the configured index exists and contains the
seed document IDs.

## 6. Ask a Dataset-Related Question

```bash
curl -X POST http://localhost:5000/api/ask \
  -H "Content-Type: application/json" \
  -d '{"question":"What markets involve CPI or inflation?","topK":5}'
```

Confirm:

- The response includes a concise grounded answer.
- The answer cites bracketed source IDs such as `[market-001]`.
- `sources` contains matching source records.
- `retrieval.documentsReturned` is greater than zero.
- Logs show retrieval count and document IDs/scores without full prompts or keys.

## 7. Ask an Unrelated Question

```bash
curl -X POST http://localhost:5000/api/ask \
  -H "Content-Type: application/json" \
  -d '{"question":"Which markets cover lunar mining permits?","topK":5}'
```

Confirm one of these safe outcomes:

- Vector search returns no sufficiently relevant documents and the API returns:
  `I do not have enough information in the indexed data to answer that.`
- Or retrieved context is weak and the final answer does not invent market data.

If unrelated questions return plausible but unsupported market facts, raise
`Rag:MinScoreThreshold`, inspect retrieved scores, and re-run the test.

## T048 Completion Checklist

- [ ] Azure OpenAI chat and embedding deployments identified.
- [ ] Azure AI Search service identified.
- [ ] Local user secrets or environment variables configured.
- [ ] API started locally.
- [ ] `GET /api/health` returned `ok`.
- [ ] `POST /api/dev/ingest` uploaded seed documents.
- [ ] In-domain ask returned source-cited grounded answer.
- [ ] Unrelated ask returned insufficiency or another safe non-invented answer.
- [ ] Logs showed RAG path without secrets or full prompts.

After every item above is checked, mark `T048` complete in
`specs/001-market-rag-assistant/tasks.md`.
