# FoundryRagBackend Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-04-27

## Active Technologies

- C# on .NET 8 LTS; SDK `8.0.126` detected in the environment + ASP.NET Core Web API, Azure.AI.OpenAI or latest compatible Azure OpenAI .NET SDK, Azure.Search.Documents, Microsoft.Extensions.Options, Microsoft.Extensions.Logging, xUnit, FluentAssertions (001-market-rag-assistant)

## Project Structure

```text
src/
tests/
```

## Commands

# Add commands for C# on .NET 8 LTS; SDK `8.0.126` detected in the environment

## Code Style

C# on .NET 8 LTS; SDK `8.0.126` detected in the environment: Follow standard conventions

## Constitution Alignment

All generated guidance MUST preserve the FoundryRagBackend constitution:
controllers handle HTTP only, normal domain answers follow the explicit RAG
pipeline, retrieved context is treated as untrusted data, Azure SDK clients stay
behind interfaces, secrets stay in configuration, structured logging avoids
sensitive data, seed ingestion remains available, and business logic remains
unit-testable without live Azure services.

## Recent Changes

- 001-market-rag-assistant: Added C# on .NET 8 LTS; SDK `8.0.126` detected in the environment + ASP.NET Core Web API, Azure.AI.OpenAI or latest compatible Azure OpenAI .NET SDK, Azure.Search.Documents, Microsoft.Extensions.Options, Microsoft.Extensions.Logging, xUnit, FluentAssertions

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
