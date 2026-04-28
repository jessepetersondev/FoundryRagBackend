namespace FoundryRag.Api.Contracts;

public sealed record SourceReference(
    string Id,
    string Title,
    string Category,
    string Source,
    double? Score);
