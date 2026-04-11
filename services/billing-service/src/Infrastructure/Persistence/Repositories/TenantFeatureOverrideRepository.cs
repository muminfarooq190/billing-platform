using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Persistence.Repositories;

public sealed class TenantFeatureOverrideRepository(BillingDbContext dbContext) : ITenantFeatureOverrideRepository
{
    public async Task AddAsync(TenantFeatureOverride entry, CancellationToken cancellationToken)
        => await dbContext.TenantFeatureOverrides.AddAsync(entry, cancellationToken);

    public async Task AddRangeAsync(IReadOnlyCollection<TenantFeatureOverride> overrides, CancellationToken cancellationToken)
        => await dbContext.TenantFeatureOverrides.AddRangeAsync(overrides, cancellationToken);

    public async Task<TenantFeatureOverride?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await dbContext.TenantFeatureOverrides.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

    public async Task<IReadOnlyList<TenantFeatureOverride>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken)
        => await dbContext.TenantFeatureOverrides
            .Where(x => x.TenantId == tenantId && x.DeletedAt == null)
            .OrderBy(x => x.FeatureKey)
            .ThenBy(x => x.EffectiveFrom)
            .ToListAsync(cancellationToken);
}
