using GeoLeadsService.Api.Contracts;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GeoLeadsService.Api.Controllers;

[ApiController]
[Route("geo-leads/saved-areas")]
public sealed class SavedGeoAreasController(ISavedGeoAreaRepository savedGeoAreaRepository, ITenantContext tenantContext) : ControllerBase
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

        var area = new SavedGeoArea(
            tenantContext.TenantId,
            request.Name,
            System.Text.Json.JsonSerializer.Serialize(new GeoPolygon(request.Geometry.Coordinates.Select(x => new GeoCoordinate(x[0], x[1])).ToList())));

        await savedGeoAreaRepository.AddAsync(area, cancellationToken);
        return Ok(ToResponse(area));
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        if (tenantContext.TenantId == Guid.Empty)
            return Unauthorized(new { error = "x-tenant-id header is required." });

        var areas = await savedGeoAreaRepository.ListByTenantAsync(tenantContext.TenantId, Math.Clamp(limit, 1, 100), cancellationToken);
        return Ok(new { count = areas.Count, areas = areas.Select(ToResponse) });
    }

    [HttpGet("{areaId:guid}")]
    public async Task<IActionResult> Get(Guid areaId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId == Guid.Empty)
            return Unauthorized(new { error = "x-tenant-id header is required." });

        var area = await savedGeoAreaRepository.GetByIdAsync(areaId, tenantContext.TenantId, cancellationToken);
        if (area is null)
            return NotFound();

        return Ok(ToResponse(area));
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

        var area = await savedGeoAreaRepository.GetByIdAsync(areaId, tenantContext.TenantId, cancellationToken);
        if (area is null)
            return NotFound();

        area.Update(
            request.Name,
            System.Text.Json.JsonSerializer.Serialize(new GeoPolygon(request.Geometry.Coordinates.Select(x => new GeoCoordinate(x[0], x[1])).ToList())));

        await savedGeoAreaRepository.UpdateAsync(area, cancellationToken);
        return Ok(ToResponse(area));
    }

    [HttpDelete("{areaId:guid}")]
    public async Task<IActionResult> Delete(Guid areaId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId == Guid.Empty)
            return Unauthorized(new { error = "x-tenant-id header is required." });

        var area = await savedGeoAreaRepository.GetByIdAsync(areaId, tenantContext.TenantId, cancellationToken);
        if (area is null)
            return NotFound();

        await savedGeoAreaRepository.DeleteAsync(area, cancellationToken);
        return NoContent();
    }

    private static object ToResponse(SavedGeoArea area)
    {
        var polygon = System.Text.Json.JsonSerializer.Deserialize<GeoPolygon>(area.GeometryJson);

        return new
        {
            areaId = area.Id,
            name = area.Name,
            createdAt = area.CreatedAt,
            updatedAt = area.UpdatedAt,
            geometry = polygon is null
                ? null
                : new
                {
                    type = "Polygon",
                    coordinates = polygon.Coordinates.Select(x => new[] { x.Longitude, x.Latitude })
                }
        };
    }
}
