using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using GeoLeadsService.Infrastructure.Persistence.Spatial;
using Microsoft.EntityFrameworkCore;

namespace GeoLeadsService.Infrastructure.Persistence.Repositories;

public sealed class PostGisGeoLeadCatalog(GeoLeadsDbContext dbContext, ILeadSourceRecordRepository leadSourceRecordRepository) : IGeoLeadCatalog
{
    public async Task<IReadOnlyList<GeoLead>> SearchAsync(GeoPolygon geometry, IReadOnlyList<string> leadTypes, int limit, CancellationToken cancellationToken)
    {
        var polygon = GeoSpatialConversions.ToPolygon(geometry);

        var query = dbContext.LeadSourceRecords.AsQueryable();
        if (leadTypes.Count > 0)
            query = query.Where(x => leadTypes.Contains(x.RawCategory));

        var spatial = await query
            .Where(x => x.RawLatitude != null && x.RawLongitude != null)
            .Where(x => EF.Property<NetTopologySuite.Geometries.Point?>(x, "Location") != null)
            .Where(x => EF.Property<NetTopologySuite.Geometries.Point>(x, "Location").Within(polygon))
            .OrderByDescending(x => x.LastSeenAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        if (spatial.Count > 0)
            return spatial.Select(MapFromSourceRecord).ToList();

        var fallback = await leadSourceRecordRepository.ListAsync(cancellationToken);
        return fallback
            .Select(MapFromSourceRecord)
            .Where(x => leadTypes.Count == 0 || leadTypes.Contains(x.LeadType, StringComparer.OrdinalIgnoreCase))
            .Take(limit)
            .ToList();
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
}
