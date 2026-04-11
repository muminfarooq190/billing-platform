using BillingService.Domain.Aggregates;

namespace BillingService.Domain.Repositories;

public interface ITenantFeatureOverrideRepository
{
    Task AddAsync(TenantFeatureOverride entry, CancellationToken cancellationToken);
    Task AddRangeAsync(IReadOnlyCollection<TenantFeatureOverride> overrides, CancellationToken cancellationToken);
    Task<TenantFeatureOverride?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenantFeatureOverride>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken);
}
