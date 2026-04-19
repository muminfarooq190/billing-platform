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
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string GeometryJson { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
}
