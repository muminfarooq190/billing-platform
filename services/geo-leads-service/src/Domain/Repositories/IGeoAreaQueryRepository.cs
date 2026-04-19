using GeoLeadsService.Application.Queries.ListGeoAreaQueries;
using GeoLeadsService.Domain.Aggregates;

namespace GeoLeadsService.Domain.Repositories;

public interface IGeoAreaQueryRepository
{
    Task AddAsync(GeoAreaQuery query, CancellationToken cancellationToken);
    Task UpdateAsync(GeoAreaQuery query, CancellationToken cancellationToken);
    Task<GeoAreaQuery?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyList<GeoAreaQuery>> ListByTenantAsync(Guid tenantId, int limit, CancellationToken cancellationToken);
    Task<IReadOnlyList<GeoAreaQueryListItem>> ListSummariesByTenantAsync(Guid tenantId, int limit, CancellationToken cancellationToken);
}
