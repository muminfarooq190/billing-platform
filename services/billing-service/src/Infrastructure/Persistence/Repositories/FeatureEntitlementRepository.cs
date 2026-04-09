using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Persistence.Repositories;

public sealed class FeatureEntitlementRepository(BillingDbContext dbContext) : IFeatureEntitlementRepository
{
    public async Task AddRangeAsync(IReadOnlyCollection<FeatureEntitlement> entitlements, CancellationToken cancellationToken)
        => await dbContext.FeatureEntitlements.AddRangeAsync(entitlements, cancellationToken);

    public async Task<IReadOnlyList<FeatureEntitlement>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken)
        => await dbContext.FeatureEntitlements
            .Where(x => x.TenantId == tenantId && x.DeletedAt == null)
            .ToListAsync(cancellationToken);
}
