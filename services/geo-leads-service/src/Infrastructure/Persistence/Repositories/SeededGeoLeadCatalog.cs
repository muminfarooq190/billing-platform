using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;

namespace GeoLeadsService.Infrastructure.Persistence.Repositories;

public sealed class SeededGeoLeadCatalog(ILeadSourceRecordRepository leadSourceRecordRepository) : IGeoLeadCatalog
{
    public async Task<IReadOnlyList<GeoLead>> SearchAsync(GeoPolygon geometry, IReadOnlyList<string> leadTypes, int limit, CancellationToken cancellationToken)
    {
        var sourceRecords = await leadSourceRecordRepository.ListAsync(cancellationToken);
        var leads = sourceRecords.Count > 0
            ? sourceRecords.Select(MapFromSourceRecord).ToList()
            : BuildFallbackLeads();

        var filtered = leads
            .Where(x => leadTypes.Count == 0 || leadTypes.Contains(x.LeadType, StringComparer.OrdinalIgnoreCase))
            .Where(x => IsInsidePolygon(x.Longitude, x.Latitude, geometry))
            .OrderByDescending(x => (x.TourismRelevanceScore * 0.35m) + (x.ContactabilityScore * 0.30m) + (x.ConfidenceScore * 0.35m))
            .Take(limit)
            .ToList();

        return filtered;
    }

    private static GeoLead MapFromSourceRecord(LeadSourceRecord record)
        => new(
            record.Id,
            record.RawName,
            record.RawCategory,
            record.RawEmail,
            record.RawPhone,
            record.RawWebsite,
            record.RawAddress ?? string.Empty,
            record.RawLatitude ?? 0m,
            record.RawLongitude ?? 0m,
            "Unknown",
            "Unknown",
            "Unknown",
            0.80m,
            string.IsNullOrWhiteSpace(record.RawEmail) && string.IsNullOrWhiteSpace(record.RawPhone) ? 0.45m : 0.75m,
            0.82m,
            [record.SourceName],
            [$"ingested from {record.SourceName}", "public source record"],
            record.LastSeenAt);

    private static List<GeoLead> BuildFallbackLeads() =>
    [
        new(Guid.NewGuid(), "Sunrise Adventures", "tour_operator", "hello@sunrise.example", "+919999000001", "https://sunrise.example", "Colaba, Mumbai", 18.9218m, 72.8347m, "Mumbai", "Maharashtra", "India", 0.87m, 0.80m, 0.91m, ["seeded-public-tourism"], ["tourism business", "email available"], DateTimeOffset.UtcNow),
        new(Guid.NewGuid(), "Harbor Stay Boutique", "hotel", "stay@harbor.example", "+919999000002", "https://harbor.example", "Fort, Mumbai", 18.9350m, 72.8355m, "Mumbai", "Maharashtra", "India", 0.82m, 0.84m, 0.88m, ["seeded-public-tourism"], ["hospitality listing", "phone and website available"], DateTimeOffset.UtcNow),
        new(Guid.NewGuid(), "Coastal Explorer", "activity_provider", "contact@coastal.example", null, "https://coastal.example", "Gateway area, Mumbai", 18.9225m, 72.8333m, "Mumbai", "Maharashtra", "India", 0.78m, 0.62m, 0.89m, ["seeded-public-tourism"], ["destination activity operator"], DateTimeOffset.UtcNow)
    ];

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
