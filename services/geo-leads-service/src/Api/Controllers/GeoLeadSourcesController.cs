using GeoLeadsService.Application.Commands.IngestLeadSources;
using GeoLeadsService.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GeoLeadsService.Api.Controllers;

[ApiController]
[Route("geo-leads/sources")]
public sealed class GeoLeadSourcesController(IMediator mediator, ILeadSourceRecordRepository leadSourceRecordRepository) : ControllerBase
{
    [HttpGet("status")]
    public async Task<IActionResult> Status([FromQuery] int limit = 20, CancellationToken cancellationToken = default)
    {
        var records = await leadSourceRecordRepository.ListRecentAsync(Math.Clamp(limit, 1, 100), cancellationToken);

        var grouped = records
            .GroupBy(x => x.SourceName)
            .Select(group => new
            {
                sourceName = group.Key,
                totalRecords = group.Count(),
                newestSeenAt = group.Max(x => x.LastSeenAt),
                oldestSeenAt = group.Min(x => x.FirstSeenAt),
                sample = group.Take(3).Select(x => new
                {
                    sourceRecordId = x.SourceRecordId,
                    rawName = x.RawName,
                    rawCategory = x.RawCategory,
                    firstSeenAt = x.FirstSeenAt,
                    lastSeenAt = x.LastSeenAt
                })
            })
            .OrderByDescending(x => x.newestSeenAt)
            .ToList();

        return Ok(new
        {
            sourceCount = grouped.Count,
            recordCount = records.Count,
            sources = grouped
        });
    }

    [HttpPost("ingest")]
    public async Task<IActionResult> Ingest(CancellationToken cancellationToken)
    {
        var count = await mediator.Send(new IngestLeadSourcesCommand(), cancellationToken);
        return Ok(new { ingested = count });
    }
}
