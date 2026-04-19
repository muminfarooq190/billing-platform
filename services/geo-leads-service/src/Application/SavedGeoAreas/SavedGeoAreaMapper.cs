using System.Text.Json;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;

namespace GeoLeadsService.Application.SavedGeoAreas;

public static class SavedGeoAreaMapper
{
    public static object ToResponse(SavedGeoArea area)
    {
        var polygon = JsonSerializer.Deserialize<GeoPolygon>(area.GeometryJson);

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
