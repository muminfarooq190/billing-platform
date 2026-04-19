using GeoLeadsService.Application.Queries.ListGeoAreaQueries;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GeoLeadsService.Infrastructure.Persistence.Repositories;

public sealed class GeoAreaQueryRepository(GeoLeadsDbContext dbContext) : IGeoAreaQueryRepository
{
    public async Task AddAsync(GeoAreaQuery query, CancellationToken cancellationToken)
    {
        dbContext.GeoAreaQueries.Add(query);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(GeoAreaQuery query, CancellationToken cancellationToken)
    {
        dbContext.GeoAreaQueries.Update(query);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<GeoAreaQuery?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken)
        => dbContext.GeoAreaQueries
            .Include(x => x.Results)
            .SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);

    public async Task<IReadOnlyList<GeoAreaQuery>> ListByTenantAsync(Guid tenantId, int limit, CancellationToken cancellationToken)
        => await dbContext.GeoAreaQueries
            .Include(x => x.Results)
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<GeoAreaQueryListItem>> ListSummariesByTenantAsync(Guid tenantId, int limit, CancellationToken cancellationToken)
    {
        var rows = await dbContext.GeoAreaQueries
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .Select(x => new
            {
                x.Id,
                Status = x.Status.ToString(),
                x.RankingMode,
                x.RequestedLimit,
                x.RequestedLeadTypesJson,
                ResultCount = x.Results.Count,
                x.CreatedAt,
                x.CompletedAt,
                PointCount = x.Geometry != null ? x.Geometry.NumPoints : (int?)null,
                MinLng = x.Geometry != null ? (decimal?)x.Geometry.EnvelopeInternal.MinX : null,
                MinLat = x.Geometry != null ? (decimal?)x.Geometry.EnvelopeInternal.MinY : null,
                MaxLng = x.Geometry != null ? (decimal?)x.Geometry.EnvelopeInternal.MaxX : null,
                MaxLat = x.Geometry != null ? (decimal?)x.Geometry.EnvelopeInternal.MaxY : null
            })
            .ToListAsync(cancellationToken);

        return rows.Select(x => new GeoAreaQueryListItem(
            x.Id,
            x.Status,
            x.RankingMode,
            x.RequestedLimit,
            System.Text.Json.JsonSerializer.Deserialize<List<string>>(x.RequestedLeadTypesJson) ?? [],
            x.ResultCount,
            x.CreatedAt,
            x.CompletedAt,
            x.PointCount,
            x.MinLng,
            x.MinLat,
            x.MaxLng,
            x.MaxLat)).ToList();
    }
}
