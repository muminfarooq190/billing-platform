using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Persistence.Repositories;

public sealed class CommercialPackageRepository(BillingDbContext dbContext) : ICommercialPackageRepository
{
    public async Task<IReadOnlyList<CommercialPackage>> ListActiveAsync(CancellationToken cancellationToken)
        => await dbContext.CommercialPackages.Where(x => x.IsActive && x.DeletedAt == null).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CommercialPackageFeature>> ListFeaturesByPackageIdsAsync(IReadOnlyCollection<Guid> packageIds, CancellationToken cancellationToken)
        => await dbContext.CommercialPackageFeatures.Where(x => packageIds.Contains(x.CommercialPackageId)).ToListAsync(cancellationToken);

    public async Task AddRangeAsync(IReadOnlyCollection<CommercialPackage> packages, CancellationToken cancellationToken)
        => await dbContext.CommercialPackages.AddRangeAsync(packages, cancellationToken);

    public async Task AddFeaturesAsync(IReadOnlyCollection<CommercialPackageFeature> features, CancellationToken cancellationToken)
        => await dbContext.CommercialPackageFeatures.AddRangeAsync(features, cancellationToken);
}
