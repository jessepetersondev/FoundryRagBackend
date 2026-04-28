using FoundryRag.Api.Models;

namespace FoundryRag.Api.Services;

public interface IAnswerGroundingValidator
{
    AnswerGroundingValidationResult Validate(string answer, IReadOnlyList<RetrievedDocument> retrievedDocuments);
}

public sealed record AnswerGroundingValidationResult(
    bool IsValid,
    bool HasCitations,
    IReadOnlySet<string> CitedSourceIds,
    IReadOnlySet<string> UnknownSourceIds);
