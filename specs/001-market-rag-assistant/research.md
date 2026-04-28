# Research: Backend RAG Assistant for Market Data

## Decision: Target .NET 8 LTS

**Rationale**: .NET SDK `8.0.126` is available in the environment and .NET 8 is
an LTS-compatible target. It satisfies the user's request for .NET 8 or latest
stable LTS-compatible version available locally.

**Alternatives considered**: .NET 9 was not selected because the local
environment only reports .NET 8, and LTS stability is preferred for an
interview-ready backend.

## Decision: ASP.NET Core Web API With Controllers

**Rationale**: Controllers map cleanly to the requested endpoints:
`POST /api/ask`, `GET /api/health`, and `POST /api/dev/ingest`. They keep HTTP
concerns at the boundary while `IRagService` and ingestion services own workflow.

**Alternatives considered**: Minimal APIs were rejected because controllers make
the separation between HTTP concerns and application services more explicit for
this architecture.

## Decision: Azure OpenAI and Azure AI Search SDKs Behind Interfaces

**Rationale**: `IEmbeddingService`, `IChatCompletionService`, and
`IVectorSearchService` allow unit tests to use fakes while production services
use Azure SDK clients. This directly satisfies the constitution's testability
and no controller-to-Azure rules.

**Alternatives considered**: Direct SDK usage in controllers or `RagService` was
rejected because it couples business workflow to Azure clients and makes tests
require live services.

## Decision: Strongly Typed Options for All External Settings

**Rationale**: `AzureOpenAiOptions`, `AzureSearchOptions`, and `RagOptions`
centralize required configuration, support validation at startup, and avoid
hard-coded secrets. Local development can use user secrets, environment
variables, or development placeholders.

**Alternatives considered**: Reading configuration keys inline was rejected
because it scatters configuration and weakens validation.

## Decision: Azure AI Search Vector Index With Document-Level Vectors

**Rationale**: The seed dataset is intentionally small, so simple document-level
indexing keeps ingestion and retrieval easy to explain. The index includes
`id`, `title`, `category`, `content`, `source`, `effectiveDate`, and an
embedding vector field sized to the configured embedding model.

**Alternatives considered**: Complex chunking and reranking were rejected as out
of scope. They add complexity without improving the small local demonstration.

## Decision: HNSW Vector Search Profile

**Rationale**: HNSW is the standard approximate nearest-neighbor profile for
vector retrieval in Azure AI Search and is suitable for top-k nearest document
lookup over the seed dataset.

**Alternatives considered**: Exhaustive vector search was not selected as the
default because HNSW better reflects a production-style vector index. Exhaustive
search may still be useful during manual troubleshooting if supported.

## Decision: Development-Only Ingestion Endpoint

**Rationale**: `POST /api/dev/ingest` matches the local-only scope and gives a
simple manual trigger for reading seed JSON, generating embeddings, ensuring the
search index, and uploading documents. It must be clearly documented as a
development endpoint.

**Alternatives considered**: Startup ingestion was rejected because it can cause
accidental repeated uploads. A separate console command was considered, but the
endpoint keeps the demo surface small.

## Decision: Prompt Builder Returns a RagPrompt Model

**Rationale**: A dedicated `IPromptBuilder` centralizes grounding,
source-citation, prompt-injection, and investment-advice restrictions. Returning
a `RagPrompt` model makes the chat service responsible only for sending the
prepared messages.

**Alternatives considered**: Building prompts inline inside `RagService` was
rejected because it makes safety rules harder to test and review.

## Decision: No-Document Retrieval Returns Insufficiency Before Chat

**Rationale**: If retrieval returns no documents, there is no context to ground
an answer. Returning the fixed insufficiency response avoids model-memory
answers and saves a chat completion call.

**Alternatives considered**: Always calling chat was rejected because it
increases hallucination risk for empty context. A configuration flag may allow
chat-on-empty later, but the default remains no chat.

## Decision: Basic Retry Wrapper for Transient Azure Failures

**Rationale**: A small retry helper around transient Azure calls improves local
demo reliability without introducing a larger resilience framework. Retry logic
must preserve cancellation and log failures without secrets.

**Alternatives considered**: Adding Polly was rejected for the initial plan
because the project should stay simple and interview-friendly.

## Decision: xUnit and FluentAssertions for Unit Tests

**Rationale**: xUnit is a common .NET test framework, and FluentAssertions keeps
test intent readable. The core logic can be tested with fake services without
Azure resources.

**Alternatives considered**: MSTest and NUnit are viable, but xUnit keeps the
test setup familiar and concise.
