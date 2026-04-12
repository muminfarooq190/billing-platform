using BillingService.Domain.Aggregates;

namespace BillingService.Domain.Repositories;

public interface ITenantUserFeatureAssignmentRepository
{
    Task AddAsync(TenantUserFeatureAssignment assignment, CancellationToken cancellationToken);
    Task<TenantUserFeatureAssignment?> GetActiveAssignmentAsync(Guid tenantId, Guid userId, string featureKey, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenantUserFeatureAssignment>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenantUserFeatureAssignment>> ListByTenantIdAndUserIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken);
    Task<int> CountActiveAssignmentsAsync(Guid tenantId, string featureKey, CancellationToken cancellationToken);
}
