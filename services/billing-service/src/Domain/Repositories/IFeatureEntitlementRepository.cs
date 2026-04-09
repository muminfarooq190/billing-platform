using BillingService.Domain.Aggregates;

namespace BillingService.Domain.Repositories;

public interface IFeatureEntitlementRepository
{
    Task AddRangeAsync(IReadOnlyCollection<FeatureEntitlement> entitlements, CancellationToken cancellationToken);
    Task<IReadOnlyList<FeatureEntitlement>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken);
}
