using GeoLeadsService.Domain.Aggregates;

namespace GeoLeadsService.Application.Abstractions;

public interface IGeoLeadCatalog
{
    Task<IReadOnlyList<GeoLead>> SearchAsync(GeoPolygon geometry, IReadOnlyList<string> leadTypes, int limit, CancellationToken cancellationToken);
}

public sealed record GeoPolygon(IReadOnlyList<GeoCoordinate> Coordinates);
public sealed record GeoCoordinate(decimal Longitude, decimal Latitude);
