using GeoLeadsService.Domain.Enums;

namespace GeoLeadsService.Domain.Aggregates;

public sealed class GeoAreaQuery
{
    private readonly List<GeoAreaQueryResult> _results = [];

    private GeoAreaQuery() { }

    public GeoAreaQuery(Guid tenantId, string geometryJson, IReadOnlyList<string> requestedLeadTypes, int requestedLimit)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        GeometryJson = geometryJson;
        RequestedLeadTypes = requestedLeadTypes;
        RequestedLimit = requestedLimit;
        Status = GeoAreaQueryStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string GeometryJson { get; private set; } = string.Empty;
    public IReadOnlyList<string> RequestedLeadTypes { get; private set; } = [];
    public int RequestedLimit { get; private set; }
    public GeoAreaQueryStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public IReadOnlyList<GeoAreaQueryResult> Results => _results;

    public void Complete(IReadOnlyCollection<GeoAreaQueryResult> results)
    {
        _results.Clear();
        _results.AddRange(results);
        Status = GeoAreaQueryStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}

public sealed record GeoAreaQueryResult(
    Guid GeoLeadId,
    int Rank,
    decimal Score,
    GeoLead Lead,
    IReadOnlyList<string> Reasoning);
