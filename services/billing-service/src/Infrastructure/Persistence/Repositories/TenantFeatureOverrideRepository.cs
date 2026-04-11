using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Persistence.Repositories;

public sealed class TenantFeatureOverrideRepository(BillingDbContext dbContext) : ITenantFeatureOverrideRepository
{
    public async Task AddRangeAsync(IReadOnlyCollection<TenantFeatureOverride> overrides, CancellationToken cancellationToken)
        => await dbContext.TenantFeatureOverrides.AddRangeAsync(overrides, cancellationToken);

    public async Task<IReadOnlyList<TenantFeatureOverride>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken)
        => await dbContext.TenantFeatureOverrides
            .Where(x => x.TenantId == tenantId && x.DeletedAt == null)
            .ToListAsync(cancellationToken);
}
