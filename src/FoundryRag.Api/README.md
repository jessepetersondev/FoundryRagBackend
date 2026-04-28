# FoundryRag.Api

This is the ASP.NET Core API project for FoundryRagBackend.

The root [README.md](../../README.md) is the canonical project guide. It covers:

- RAG architecture and request flow
- required Azure OpenAI and Azure AI Search resources
- local configuration with user secrets
- seed ingestion
- frontend and API usage
- grounding behavior and safety controls
- troubleshooting and manual Azure verification

Run from the repository root:

```bash
dotnet run --project src/FoundryRag.Api --urls http://localhost:5000
```
