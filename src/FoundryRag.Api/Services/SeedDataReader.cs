using System.Text.Json;
using FoundryRag.Api.Infrastructure;
using FoundryRag.Api.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace FoundryRag.Api.Services;

public sealed class SeedDataReader : ISeedDataReader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<SeedDataReader> _logger;

    public SeedDataReader(IWebHostEnvironment environment, ILogger<SeedDataReader>? logger = null)
    {
        _environment = environment;
        _logger = logger ?? NullLogger<SeedDataReader>.Instance;
    }

    public async Task<IReadOnlyList<MarketDocument>> ReadSeedDataAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(_environment.ContentRootPath, "Data", "seed-markets.json");
        if (!File.Exists(path))
        {
            _logger.LogError("Seed data file was not found at {SeedDataPath}", path);
            throw new SeedDataException("Seed data could not be loaded.");
        }

        try
        {
            await using var stream = File.OpenRead(path);
            var documents = await JsonSerializer.DeserializeAsync<List<MarketDocument>>(stream, JsonOptions, cancellationToken)
                ?? throw new SeedDataException("Seed data file did not contain any documents.");

            Validate(documents);
            return documents;
        }
        catch (JsonException ex)
        {
            throw new SeedDataException("Seed data file is not valid JSON.", ex);
        }
        catch (IOException ex)
        {
            throw new SeedDataException("Seed data file could not be read.", ex);
        }
    }

    private static void Validate(IReadOnlyList<MarketDocument> documents)
    {
        if (documents.Count == 0)
        {
            throw new SeedDataException("Seed data file must contain at least one document.");
        }

        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var document in documents)
        {
            if (string.IsNullOrWhiteSpace(document.Id) ||
                string.IsNullOrWhiteSpace(document.Title) ||
                string.IsNullOrWhiteSpace(document.Category) ||
                string.IsNullOrWhiteSpace(document.Description) ||
                string.IsNullOrWhiteSpace(document.Rules) ||
                document.Outcomes.Count == 0 ||
                string.IsNullOrWhiteSpace(document.Source) ||
                (string.IsNullOrWhiteSpace(document.EffectiveDate) && string.IsNullOrWhiteSpace(document.CloseDate)))
            {
                throw new SeedDataException($"Seed document '{document.Id}' is missing required fields.");
            }

            if (!seenIds.Add(document.Id))
            {
                throw new SeedDataException($"Duplicate seed document id '{document.Id}'.");
            }
        }
    }
}
