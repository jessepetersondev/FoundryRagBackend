namespace FoundryRag.Api.Contracts;

public sealed record AskRequest(string Question, int? TopK);
