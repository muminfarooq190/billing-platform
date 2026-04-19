using GeoLeadsService.Domain.Aggregates;

namespace GeoLeadsService.Domain.Repositories;

public interface ISavedGeoAreaRepository
{
    Task AddAsync(SavedGeoArea area, CancellationToken cancellationToken);
    Task UpdateAsync(SavedGeoArea area, CancellationToken cancellationToken);
    Task DeleteAsync(SavedGeoArea area, CancellationToken cancellationToken);
    Task<SavedGeoArea?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SavedGeoArea>> ListByTenantAsync(Guid tenantId, int limit, CancellationToken cancellationToken);
}
