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
    public string? Ticker { get; init; }
    public string? SeriesTicker { get; init; }
    public string? MarketType { get; init; }
    public string? Status { get; init; }
    public string? EffectiveDate { get; init; }
    public string? CloseDate { get; init; }
    public string? EventDate { get; init; }
    public string? ExpirationDate { get; init; }
    public string? ResolutionSource { get; init; }
    public int? YesBidCents { get; init; }
    public int? YesAskCents { get; init; }
    public int? NoBidCents { get; init; }
    public int? NoAskCents { get; init; }
    public int? LastTradePriceCents { get; init; }
    public long? Volume { get; init; }
    public long? OpenInterest { get; init; }
    public long? LiquidityCents { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public string? Notes { get; init; }
}
