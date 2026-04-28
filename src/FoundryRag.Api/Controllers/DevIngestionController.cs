using FoundryRag.Api.Contracts;
using FoundryRag.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoundryRag.Api.Controllers;

[ApiController]
[Route("api/dev/ingest")]
public sealed class DevIngestionController : ControllerBase
{
    private readonly IDocumentIngestionService _documentIngestionService;
    private readonly IWebHostEnvironment _environment;

    public DevIngestionController(IDocumentIngestionService documentIngestionService, IWebHostEnvironment environment)
    {
        _documentIngestionService = documentIngestionService;
        _environment = environment;
    }

    [HttpPost]
    [ProducesResponseType(typeof(IngestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IngestResponse>> Ingest(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var response = await _documentIngestionService.IngestSeedDataAsync(cancellationToken);
        return Ok(response);
    }
}
