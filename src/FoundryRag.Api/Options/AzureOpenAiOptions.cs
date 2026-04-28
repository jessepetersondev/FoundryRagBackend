namespace FoundryRag.Api.Options;

public sealed class AzureOpenAiOptions
{
    public const string SectionName = "AzureOpenAi";

    public string Endpoint { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string ChatDeploymentName { get; init; } = "gpt-4o-mini";
    public string EmbeddingDeploymentName { get; init; } = "text-embedding-3-small";

    public static bool IsValid(AzureOpenAiOptions options) =>
        Uri.TryCreate(options.Endpoint, UriKind.Absolute, out _) &&
        !string.IsNullOrWhiteSpace(options.ApiKey) &&
        !string.IsNullOrWhiteSpace(options.ChatDeploymentName) &&
        !string.IsNullOrWhiteSpace(options.EmbeddingDeploymentName);
}
