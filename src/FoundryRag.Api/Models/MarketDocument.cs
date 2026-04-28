namespace FoundryRag.Api.Models;

public sealed record MarketDocument
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Category { get; init; }
    public required string Description { get; init; }
    public required string Rules { get; init; }
    public required IReadOnlyList<string> Outcomes { get; init; }
    public required string Source { get; init; }
    public string? EffectiveDate { get; init; }
    public string? CloseDate { get; init; }
    public string? Notes { get; init; }
}
