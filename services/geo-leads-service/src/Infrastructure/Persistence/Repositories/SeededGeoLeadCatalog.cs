using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;

namespace GeoLeadsService.Infrastructure.Persistence.Repositories;

public sealed class SeededGeoLeadCatalog : IGeoLeadCatalog
{
    private static readonly IReadOnlyList<GeoLead> Leads =
    [
        new(Guid.NewGuid(), "Sunrise Adventures", "tour_operator", "hello@sunrise.example", "+919999000001", "https://sunrise.example", "Colaba, Mumbai", 18.9218m, 72.8347m, "Mumbai", "Maharashtra", "India", 0.87m, 0.80m, 0.91m, ["osm", "public-website"], ["tourism business", "email available"], DateTimeOffset.UtcNow),
        new(Guid.NewGuid(), "Harbor Stay Boutique", "hotel", "stay@harbor.example", "+919999000002", "https://harbor.example", "Fort, Mumbai", 18.9350m, 72.8355m, "Mumbai", "Maharashtra", "India", 0.82m, 0.84m, 0.88m, ["osm", "public-directory"], ["hospitality listing", "phone and website available"], DateTimeOffset.UtcNow),
        new(Guid.NewGuid(), "Coastal Explorer", "activity_provider", "contact@coastal.example", null, "https://coastal.example", "Gateway area, Mumbai", 18.9225m, 72.8333m, "Mumbai", "Maharashtra", "India", 0.78m, 0.62m, 0.89m, ["public-website"], ["destination activity operator"], DateTimeOffset.UtcNow)
    ];

    public Task<IReadOnlyList<GeoLead>> SearchAsync(GeoPolygon geometry, IReadOnlyList<string> leadTypes, int limit, CancellationToken cancellationToken)
    {
        var filtered = Leads
            .Where(x => leadTypes.Count == 0 || leadTypes.Contains(x.LeadType, StringComparer.OrdinalIgnoreCase))
            .Where(x => IsInsidePolygon(x.Longitude, x.Latitude, geometry))
            .OrderByDescending(x => (x.TourismRelevanceScore * 0.35m) + (x.ContactabilityScore * 0.30m) + (x.ConfidenceScore * 0.35m))
            .Take(limit)
            .ToList();

        return Task.FromResult<IReadOnlyList<GeoLead>>(filtered);
    }

    private static bool IsInsidePolygon(decimal longitude, decimal latitude, GeoPolygon polygon)
    {
        var inside = false;
        for (int i = 0, j = polygon.Coordinates.Count - 1; i < polygon.Coordinates.Count; j = i++)
        {
            var xi = polygon.Coordinates[i].Longitude;
            var yi = polygon.Coordinates[i].Latitude;
            var xj = polygon.Coordinates[j].Longitude;
            var yj = polygon.Coordinates[j].Latitude;

            var intersects = ((yi > latitude) != (yj > latitude)) &&
                             (longitude < (xj - xi) * (latitude - yi) / ((yj - yi) == 0 ? 0.0000001m : (yj - yi)) + xi);
            if (intersects)
                inside = !inside;
        }

        return inside;
    }
}
