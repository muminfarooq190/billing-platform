namespace GeoLeadsService.Api.Contracts;

public sealed record SubmitGeoAreaQueryRequest(
    GeometryPayload Geometry,
    IReadOnlyList<string>? LeadTypes,
    int? Limit,
    string? RankingMode);

public sealed record GeometryPayload(string Type, IReadOnlyList<IReadOnlyList<decimal>> Coordinates);

public sealed record GeoAreaQueryResponse(Guid QueryId, string Status, int Count);
