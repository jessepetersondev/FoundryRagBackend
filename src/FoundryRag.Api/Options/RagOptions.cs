namespace FoundryRag.Api.Options;

public sealed class RagOptions
{
    public const string SectionName = "Rag";

    public int DefaultTopK { get; init; } = 5;
    public int MaxTopK { get; init; } = 10;
    public double MinScoreThreshold { get; init; } = 0.0;
    public float Temperature { get; init; } = 0.1f;
    public int MaxOutputTokens { get; init; } = 800;
    public int MaxQuestionLength { get; init; } = 1000;
    public int MaxContextCharactersPerDocument { get; init; } = 1800;
    public int EmbeddingDimensions { get; init; } = 1536;

    public static bool IsValid(RagOptions options) =>
        options.DefaultTopK >= 1 &&
        options.MaxTopK >= options.DefaultTopK &&
        options.MinScoreThreshold >= 0 &&
        options.Temperature >= 0 &&
        options.MaxOutputTokens > 0 &&
        options.MaxQuestionLength > 0 &&
        options.MaxContextCharactersPerDocument > 0 &&
        options.EmbeddingDimensions > 0;
}
