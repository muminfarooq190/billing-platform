using GeoLeadsService.Infrastructure.Persistence.Spatial;
using NetTopologySuite.Geometries;

namespace GeoLeadsService.Domain.Aggregates;

public sealed class SavedGeoArea
{
    private SavedGeoArea() { }

    public SavedGeoArea(Guid tenantId, string name, string geometryJson)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Name = name.Trim();
        GeometryJson = geometryJson;
        var polygon = System.Text.Json.JsonSerializer.Deserialize<GeoLeadsService.Application.Abstractions.GeoPolygon>(geometryJson);
        Geometry = GeoSpatialConversions.TryToPolygon(polygon);
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string GeometryJson { get; private set; } = string.Empty;
    public Polygon? Geometry { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(string name, string geometryJson)
    {
        Name = name.Trim();
        GeometryJson = geometryJson;
        var polygon = System.Text.Json.JsonSerializer.Deserialize<GeoLeadsService.Application.Abstractions.GeoPolygon>(geometryJson);
        Geometry = GeoSpatialConversions.TryToPolygon(polygon);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
