using System.Text.RegularExpressions;
using FoundryRag.Api.Models;

namespace FoundryRag.Api.Services;

public sealed partial class AnswerGroundingValidator : IAnswerGroundingValidator
{
    public AnswerGroundingValidationResult Validate(string answer, IReadOnlyList<RetrievedDocument> retrievedDocuments)
    {
        ArgumentNullException.ThrowIfNull(answer);
        ArgumentNullException.ThrowIfNull(retrievedDocuments);

        var allowedIds = retrievedDocuments
            .Select(document => document.Id)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var citedIds = SourceIdRegex()
            .Matches(answer)
            .Select(match => match.Groups["sourceId"].Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var unknownIds = citedIds
            .Where(citedId => !allowedIds.Contains(citedId))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var hasCitations = citedIds.Count > 0;
        var isValid = retrievedDocuments.Count == 0 || (hasCitations && unknownIds.Count == 0);

        return new AnswerGroundingValidationResult(isValid, hasCitations, citedIds, unknownIds);
    }

    [GeneratedRegex(@"\[(?<sourceId>[A-Za-z0-9][A-Za-z0-9._:-]*)\]", RegexOptions.CultureInvariant)]
    private static partial Regex SourceIdRegex();
}
