using BillingService.Domain.Aggregates;

namespace BillingService.Domain.Repositories;

public interface ITenantFeatureOverrideRepository
{
    Task AddRangeAsync(IReadOnlyCollection<TenantFeatureOverride> overrides, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenantFeatureOverride>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken);
}
