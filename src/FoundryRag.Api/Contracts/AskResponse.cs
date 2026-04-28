namespace FoundryRag.Api.Contracts;

public sealed record AskResponse(
    string Answer,
    IReadOnlyList<SourceReference> Sources,
    RetrievalMetadata Retrieval);
