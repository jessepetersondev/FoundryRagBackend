using FoundryRag.Api.Contracts;
using FoundryRag.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoundryRag.Api.Controllers;

[ApiController]
[Route("api/ask")]
public sealed class AskController : ControllerBase
{
    private readonly IRagService _ragService;

    public AskController(IRagService ragService)
    {
        _ragService = ragService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AskResponse>> Ask(AskRequest request, CancellationToken cancellationToken)
    {
        var response = await _ragService.AskAsync(request, cancellationToken);
        return Ok(response);
    }
}
