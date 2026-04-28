namespace FoundryRag.Api.Contracts;

public sealed record IngestResponse(int DocumentsRead, int DocumentsUploaded, string IndexName);
