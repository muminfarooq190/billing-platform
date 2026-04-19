using GeoLeadsService.Domain.Enums;

namespace GeoLeadsService.Domain.Aggregates;

public sealed class GeoAreaQuery
{
    private readonly List<GeoAreaQueryResult> _results = [];

    private GeoAreaQuery() { }

    public GeoAreaQuery(Guid tenantId, string geometryJson, IReadOnlyList<string> requestedLeadTypes, int requestedLimit, string? rankingMode)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        GeometryJson = geometryJson;
        RequestedLeadTypesJson = System.Text.Json.JsonSerializer.Serialize(requestedLeadTypes);
        RequestedLimit = requestedLimit;
        RankingMode = GeoLeadsService.Application.GeoLeadRanking.NormalizeMode(rankingMode);
        Status = GeoAreaQueryStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string GeometryJson { get; private set; } = string.Empty;
    public string RequestedLeadTypesJson { get; private set; } = "[]";
    public int RequestedLimit { get; private set; }
    public string RankingMode { get; private set; } = "relevance";
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

public sealed class GeoAreaQueryResult
{
    private GeoAreaQueryResult() { }

    public GeoAreaQueryResult(Guid geoAreaQueryId, int rank, decimal score, GeoLead lead, IReadOnlyList<string> reasoning)
    {
        Id = Guid.NewGuid();
        GeoAreaQueryId = geoAreaQueryId;
        GeoLeadId = lead.Id;
        Rank = rank;
        Score = score;
        CanonicalName = lead.CanonicalName;
        LeadType = lead.LeadType;
        PrimaryEmail = lead.PrimaryEmail;
        PrimaryPhone = lead.PrimaryPhone;
        Website = lead.Website;
        Address = lead.Address;
        City = lead.City;
        Region = lead.Region;
        Country = lead.Country;
        Latitude = lead.Latitude;
        Longitude = lead.Longitude;
        SourcesJson = System.Text.Json.JsonSerializer.Serialize(lead.Sources);
        ReasoningJson = System.Text.Json.JsonSerializer.Serialize(reasoning);
    }

    public Guid Id { get; private set; }
    public Guid GeoAreaQueryId { get; private set; }
    public Guid GeoLeadId { get; private set; }
    public int Rank { get; private set; }
    public decimal Score { get; private set; }
    public string CanonicalName { get; private set; } = string.Empty;
    public string LeadType { get; private set; } = string.Empty;
    public string? PrimaryEmail { get; private set; }
    public string? PrimaryPhone { get; private set; }
    public string? Website { get; private set; }
    public string Address { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string Region { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public string SourcesJson { get; private set; } = "[]";
    public string ReasoningJson { get; private set; } = "[]";

    public GeoLead ToLead()
        => new(
            GeoLeadId,
            CanonicalName,
            LeadType,
            PrimaryEmail,
            PrimaryPhone,
            Website,
            Address,
            Latitude,
            Longitude,
            City,
            Region,
            Country,
            0m,
            0m,
            0m,
            System.Text.Json.JsonSerializer.Deserialize<List<string>>(SourcesJson) ?? [],
            System.Text.Json.JsonSerializer.Deserialize<List<string>>(ReasoningJson) ?? [],
            DateTimeOffset.UtcNow);

    public IReadOnlyList<string> GetReasoning()
        => System.Text.Json.JsonSerializer.Deserialize<List<string>>(ReasoningJson) ?? [];
}
