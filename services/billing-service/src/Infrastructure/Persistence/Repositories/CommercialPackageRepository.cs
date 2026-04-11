using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Persistence.Repositories;

public sealed class CommercialPackageRepository(BillingDbContext dbContext) : ICommercialPackageRepository
{
    public async Task<IReadOnlyList<CommercialPackage>> ListAsync(CancellationToken cancellationToken)
        => await dbContext.CommercialPackages
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CommercialPackage>> ListActiveAsync(CancellationToken cancellationToken)
        => await dbContext.CommercialPackages.Where(x => x.IsActive && x.DeletedAt == null).ToListAsync(cancellationToken);

    public async Task<CommercialPackage?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await dbContext.CommercialPackages.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

    public async Task<IReadOnlyList<CommercialPackageFeature>> ListFeaturesByPackageIdsAsync(IReadOnlyCollection<Guid> packageIds, CancellationToken cancellationToken)
        => await dbContext.CommercialPackageFeatures.Where(x => packageIds.Contains(x.CommercialPackageId)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CommercialPackageFeature>> ListFeaturesByPackageIdAsync(Guid packageId, CancellationToken cancellationToken)
        => await dbContext.CommercialPackageFeatures.Where(x => x.CommercialPackageId == packageId).OrderBy(x => x.FeatureKey).ToListAsync(cancellationToken);

    public async Task AddAsync(CommercialPackage package, CancellationToken cancellationToken)
        => await dbContext.CommercialPackages.AddAsync(package, cancellationToken);

    public async Task AddRangeAsync(IReadOnlyCollection<CommercialPackage> packages, CancellationToken cancellationToken)
        => await dbContext.CommercialPackages.AddRangeAsync(packages, cancellationToken);

    public async Task AddFeaturesAsync(IReadOnlyCollection<CommercialPackageFeature> features, CancellationToken cancellationToken)
        => await dbContext.CommercialPackageFeatures.AddRangeAsync(features, cancellationToken);

    public Task RemoveFeaturesAsync(IReadOnlyCollection<CommercialPackageFeature> features, CancellationToken cancellationToken)
    {
        dbContext.CommercialPackageFeatures.RemoveRange(features);
        return Task.CompletedTask;
    }
}
