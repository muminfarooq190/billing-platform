using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Persistence.Repositories;

public sealed class FeatureCatalogRepository(BillingDbContext dbContext) : IFeatureCatalogRepository
{
    public async Task<IReadOnlyList<FeatureCatalogEntry>> ListAsync(CancellationToken cancellationToken)
        => await dbContext.FeatureCatalog
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.Service)
            .ThenBy(x => x.Category)
            .ThenBy(x => x.FeatureKey)
            .ToListAsync(cancellationToken);

    public async Task<FeatureCatalogEntry?> GetByFeatureKeyAsync(string featureKey, CancellationToken cancellationToken)
        => await dbContext.FeatureCatalog
            .FirstOrDefaultAsync(x => x.FeatureKey == featureKey && x.DeletedAt == null, cancellationToken);

    public async Task AddAsync(FeatureCatalogEntry entry, CancellationToken cancellationToken)
        => await dbContext.FeatureCatalog.AddAsync(entry, cancellationToken);
}
