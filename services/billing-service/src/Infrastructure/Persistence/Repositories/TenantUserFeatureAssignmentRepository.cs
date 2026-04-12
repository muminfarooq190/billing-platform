using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Persistence.Repositories;

public sealed class TenantUserFeatureAssignmentRepository(BillingDbContext dbContext) : ITenantUserFeatureAssignmentRepository
{
    public async Task AddAsync(TenantUserFeatureAssignment assignment, CancellationToken cancellationToken)
        => await dbContext.TenantUserFeatureAssignments.AddAsync(assignment, cancellationToken);

    public async Task<TenantUserFeatureAssignment?> GetActiveAssignmentAsync(Guid tenantId, Guid userId, string featureKey, CancellationToken cancellationToken)
        => await dbContext.TenantUserFeatureAssignments
            .Where(x => x.TenantId == tenantId && x.UserId == userId && x.FeatureKey == featureKey && x.DeletedAt == null && x.Status == "Active")
            .OrderByDescending(x => x.AssignedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<TenantUserFeatureAssignment>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken)
        => await dbContext.TenantUserFeatureAssignments
            .Where(x => x.TenantId == tenantId && x.DeletedAt == null)
            .OrderBy(x => x.FeatureKey)
            .ThenBy(x => x.UserId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TenantUserFeatureAssignment>> ListByTenantIdAndUserIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken)
        => await dbContext.TenantUserFeatureAssignments
            .Where(x => x.TenantId == tenantId && x.UserId == userId && x.DeletedAt == null)
            .OrderBy(x => x.FeatureKey)
            .ThenByDescending(x => x.AssignedAt)
            .ToListAsync(cancellationToken);

    public async Task<int> CountActiveAssignmentsAsync(Guid tenantId, string featureKey, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        return await dbContext.TenantUserFeatureAssignments.CountAsync(x =>
            x.TenantId == tenantId
            && x.FeatureKey == featureKey
            && x.DeletedAt == null
            && x.Status == "Active"
            && x.EffectiveFrom <= now
            && (x.EffectiveTo == null || x.EffectiveTo >= now), cancellationToken);
    }
}
