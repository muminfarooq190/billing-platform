using BillingService.Domain.Aggregates;

namespace BillingService.Domain.Repositories;

public interface ITenantSubscriptionPackageRepository
{
    Task<IReadOnlyList<TenantSubscriptionPackage>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<TenantSubscriptionPackage?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(TenantSubscriptionPackage assignment, CancellationToken cancellationToken);
    Task AddRangeAsync(IReadOnlyCollection<TenantSubscriptionPackage> assignments, CancellationToken cancellationToken);
}
