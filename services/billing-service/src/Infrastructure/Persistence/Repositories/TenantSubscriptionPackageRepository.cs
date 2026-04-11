using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Persistence.Repositories;

public sealed class TenantSubscriptionPackageRepository(BillingDbContext dbContext) : ITenantSubscriptionPackageRepository
{
    public async Task<IReadOnlyList<TenantSubscriptionPackage>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken)
        => await dbContext.TenantSubscriptionPackages.Where(x => x.TenantId == tenantId && x.DeletedAt == null).OrderBy(x => x.EffectiveFrom).ToListAsync(cancellationToken);

    public async Task<TenantSubscriptionPackage?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await dbContext.TenantSubscriptionPackages.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

    public async Task AddAsync(TenantSubscriptionPackage assignment, CancellationToken cancellationToken)
        => await dbContext.TenantSubscriptionPackages.AddAsync(assignment, cancellationToken);

    public async Task AddRangeAsync(IReadOnlyCollection<TenantSubscriptionPackage> assignments, CancellationToken cancellationToken)
        => await dbContext.TenantSubscriptionPackages.AddRangeAsync(assignments, cancellationToken);
}
