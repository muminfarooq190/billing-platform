using GeoLeadsService.Api.Contracts;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Application.Commands.IngestLeadSources;
using GeoLeadsService.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GeoLeadsService.Api.Controllers;

[ApiController]
[Route("geo-leads/sources")]
public sealed class GeoLeadSourcesController(
    IMediator mediator,
    ILeadSourceRecordRepository leadSourceRecordRepository,
    ILeadSourceIngestionRunRepository leadSourceIngestionRunRepository) : ControllerBase
{
    [HttpGet("status")]
    public async Task<IActionResult> Status([FromQuery] int limit = 20, CancellationToken cancellationToken = default)
    {
        var records = await leadSourceRecordRepository.ListRecentAsync(Math.Clamp(limit, 1, 100), cancellationToken);
        var runs = await leadSourceIngestionRunRepository.ListRecentAsync(Math.Clamp(limit, 1, 100), cancellationToken);

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
            recentRuns = runs.Select(x => new
            {
                runId = x.Id,
                sourceName = x.SourceName,
                status = x.Status,
                fetchedCount = x.FetchedCount,
                errorMessage = x.ErrorMessage,
                startedAt = x.StartedAt,
                completedAt = x.CompletedAt
            }),
            sources = grouped
        });
    }

    [HttpPost("ingest")]
    public async Task<IActionResult> Ingest(CancellationToken cancellationToken)
    {
        var count = await mediator.Send(new IngestLeadSourcesCommand(), cancellationToken);
        return Ok(new { ingested = count });
    }

    /// <summary>
    /// Scope-aware ingestion. Adapters that respect a bounding box hint
    /// (Overpass etc.) restrict their fetch to the polygon; static adapters
    /// return everything they know.
    /// </summary>
    [HttpPost("ingest-area")]
    public async Task<IActionResult> IngestArea([FromBody] IngestAreaRequest request, CancellationToken cancellationToken)
    {
        if (!request.Geometry.IsValidPolygon(out var geometryError))
            return BadRequest(new { error = geometryError });

        var polygon = new GeoPolygon(request.Geometry.Coordinates.Select(x => new GeoCoordinate(x[0], x[1])).ToList());
        var bbox = GeoBoundingBox.FromPolygon(polygon);
        var count = await mediator.Send(new IngestLeadSourcesCommand(bbox), cancellationToken);
        return Ok(new
        {
            ingested = count,
            boundingBox = new { bbox.MinLongitude, bbox.MinLatitude, bbox.MaxLongitude, bbox.MaxLatitude },
        });
    }
}
