using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Persistence.Repositories;

public sealed class TenantSubscriptionPackageRepository(BillingDbContext dbContext) : ITenantSubscriptionPackageRepository
{
    public async Task<IReadOnlyList<TenantSubscriptionPackage>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken)
        => await dbContext.TenantSubscriptionPackages.Where(x => x.TenantId == tenantId && x.DeletedAt == null).ToListAsync(cancellationToken);

    public async Task AddRangeAsync(IReadOnlyCollection<TenantSubscriptionPackage> assignments, CancellationToken cancellationToken)
        => await dbContext.TenantSubscriptionPackages.AddRangeAsync(assignments, cancellationToken);
}
