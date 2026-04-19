namespace GeoLeadsService.Application.Queries.ListGeoAreaQueries;

public sealed record GeoAreaQueryListItem(
    Guid QueryId,
    string Status,
    string RankingMode,
    int RequestedLimit,
    IReadOnlyList<string> RequestedLeadTypes,
    int ResultCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    int? PointCount,
    decimal? MinLng,
    decimal? MinLat,
    decimal? MaxLng,
    decimal? MaxLat);
