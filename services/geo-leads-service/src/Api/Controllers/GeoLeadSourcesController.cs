using GeoLeadsService.Application.Commands.IngestLeadSources;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GeoLeadsService.Api.Controllers;

[ApiController]
[Route("geo-leads/sources")]
public sealed class GeoLeadSourcesController(IMediator mediator) : ControllerBase
{
    [HttpPost("ingest")]
    public async Task<IActionResult> Ingest(CancellationToken cancellationToken)
    {
        var count = await mediator.Send(new IngestLeadSourcesCommand(), cancellationToken);
        return Ok(new { ingested = count });
    }
}
