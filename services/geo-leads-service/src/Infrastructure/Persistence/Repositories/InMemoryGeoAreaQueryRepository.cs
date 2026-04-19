using System.Collections.Concurrent;
using GeoLeadsService.Application.Queries.ListGeoAreaQueries;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;

namespace GeoLeadsService.Infrastructure.Persistence.Repositories;

public sealed class InMemoryGeoAreaQueryRepository : IGeoAreaQueryRepository
{
    private static readonly ConcurrentDictionary<Guid, GeoAreaQuery> Store = new();

    public Task AddAsync(GeoAreaQuery query, CancellationToken cancellationToken)
    {
        Store[query.Id] = query;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(GeoAreaQuery query, CancellationToken cancellationToken)
    {
        Store[query.Id] = query;
        return Task.CompletedTask;
    }

    public Task<GeoAreaQuery?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken)
    {
        if (Store.TryGetValue(id, out var query) && query.TenantId == tenantId)
            return Task.FromResult<GeoAreaQuery?>(query);

        return Task.FromResult<GeoAreaQuery?>(null);
    }

    public Task<IReadOnlyList<GeoAreaQuery>> ListByTenantAsync(Guid tenantId, int limit, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<GeoAreaQuery>>(Store.Values
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToList());

    public Task<IReadOnlyList<GeoAreaQueryListItem>> ListSummariesByTenantAsync(Guid tenantId, int limit, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<GeoAreaQueryListItem>>(Store.Values
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .Select(x => new GeoAreaQueryListItem(
                x.Id,
                x.Status.ToString(),
                x.RankingMode,
                x.RequestedLimit,
                System.Text.Json.JsonSerializer.Deserialize<List<string>>(x.RequestedLeadTypesJson) ?? [],
                x.Results.Count,
                x.CreatedAt,
                x.CompletedAt,
                null,
                null,
                null,
                null,
                null))
            .ToList());
}
