namespace FoundryRag.Api.Models;

public sealed record RetrievedDocument(
    string Id,
    string Title,
    string Category,
    string Content,
    string Source,
    double? Score);
