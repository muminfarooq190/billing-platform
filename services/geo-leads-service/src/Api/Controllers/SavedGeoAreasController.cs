using GeoLeadsService.Api.Contracts;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Application.Commands.CreateSavedGeoArea;
using GeoLeadsService.Application.Commands.DeleteSavedGeoArea;
using GeoLeadsService.Application.Commands.RunSavedGeoAreaQuery;
using GeoLeadsService.Application.Commands.UpdateSavedGeoArea;
using GeoLeadsService.Application.Queries.GetSavedGeoAreaById;
using GeoLeadsService.Application.Queries.ListSavedGeoAreas;
using GeoLeadsService.Application.SavedGeoAreas;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GeoLeadsService.Api.Controllers;

[ApiController]
[Route("geo-leads/saved-areas")]
public sealed class SavedGeoAreasController(ITenantContext tenantContext, IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveGeoAreaRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId == Guid.Empty)
            return Unauthorized(new { error = "x-tenant-id header is required." });

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "name is required." });

        if (!request.Geometry.IsValidPolygon(out var geometryError))
            return BadRequest(new { error = geometryError });

        var area = await mediator.Send(
            new CreateSavedGeoAreaCommand(
                tenantContext.TenantId,
                request.Name,
                new GeoPolygon(request.Geometry.Coordinates.Select(x => new GeoCoordinate(x[0], x[1])).ToList())),
            cancellationToken);

        return Ok(SavedGeoAreaMapper.ToResponse(area));
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        if (tenantContext.TenantId == Guid.Empty)
            return Unauthorized(new { error = "x-tenant-id header is required." });

        var areas = await mediator.Send(new ListSavedGeoAreasQuery(tenantContext.TenantId, Math.Clamp(limit, 1, 100)), cancellationToken);
        return Ok(new { count = areas.Count, areas = areas.Select(SavedGeoAreaMapper.ToResponse) });
    }

    [HttpGet("{areaId:guid}")]
    public async Task<IActionResult> Get(Guid areaId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId == Guid.Empty)
            return Unauthorized(new { error = "x-tenant-id header is required." });

        var area = await mediator.Send(new GetSavedGeoAreaByIdQuery(tenantContext.TenantId, areaId), cancellationToken);
        if (area is null)
            return NotFound();

        return Ok(SavedGeoAreaMapper.ToResponse(area));
    }

    [HttpPut("{areaId:guid}")]
    public async Task<IActionResult> Update(Guid areaId, [FromBody] UpdateGeoAreaRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId == Guid.Empty)
            return Unauthorized(new { error = "x-tenant-id header is required." });

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "name is required." });

        if (!request.Geometry.IsValidPolygon(out var geometryError))
            return BadRequest(new { error = geometryError });

        var area = await mediator.Send(
            new UpdateSavedGeoAreaCommand(
                tenantContext.TenantId,
                areaId,
                request.Name,
                new GeoPolygon(request.Geometry.Coordinates.Select(x => new GeoCoordinate(x[0], x[1])).ToList())),
            cancellationToken);

        if (area is null)
            return NotFound();

        return Ok(SavedGeoAreaMapper.ToResponse(area));
    }

    [HttpPost("{areaId:guid}/run-query")]
    public async Task<IActionResult> RunQuery(Guid areaId, [FromBody] RunSavedGeoAreaQueryRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId == Guid.Empty)
            return Unauthorized(new { error = "x-tenant-id header is required." });

        var result = await mediator.Send(
            new RunSavedGeoAreaQueryCommand(
                tenantContext.TenantId,
                areaId,
                request.LeadTypes?.ToList() ?? [],
                Math.Clamp(request.Limit ?? 50, 1, 500),
                request.RankingMode),
            cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(new GeoAreaQueryResponse(result.Value.QueryId, "Completed", result.Value.Count));
    }

    [HttpDelete("{areaId:guid}")]
    public async Task<IActionResult> Delete(Guid areaId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId == Guid.Empty)
            return Unauthorized(new { error = "x-tenant-id header is required." });

        var deleted = await mediator.Send(new DeleteSavedGeoAreaCommand(tenantContext.TenantId, areaId), cancellationToken);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
