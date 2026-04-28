namespace FoundryRag.Api.Models;

public sealed record IndexedDocument(
    string Id,
    string Title,
    string Category,
    string Content,
    string Source,
    string? EffectiveDate,
    IReadOnlyList<float> Embedding);
