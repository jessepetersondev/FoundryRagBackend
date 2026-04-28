namespace FoundryRag.Api.Options;

public sealed class AzureSearchOptions
{
    public const string SectionName = "AzureSearch";

    public string Endpoint { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string IndexName { get; init; } = "market-rag-index";

    public static bool IsValid(AzureSearchOptions options) =>
        Uri.TryCreate(options.Endpoint, UriKind.Absolute, out _) &&
        !string.IsNullOrWhiteSpace(options.ApiKey) &&
        !string.IsNullOrWhiteSpace(options.IndexName);
}
