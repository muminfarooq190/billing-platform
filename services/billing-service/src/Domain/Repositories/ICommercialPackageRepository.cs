using BillingService.Domain.Aggregates;

namespace BillingService.Domain.Repositories;

public interface ICommercialPackageRepository
{
    Task<IReadOnlyList<CommercialPackage>> ListAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<CommercialPackage>> ListActiveAsync(CancellationToken cancellationToken);
    Task<CommercialPackage?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<CommercialPackageFeature>> ListFeaturesByPackageIdsAsync(IReadOnlyCollection<Guid> packageIds, CancellationToken cancellationToken);
    Task<IReadOnlyList<CommercialPackageFeature>> ListFeaturesByPackageIdAsync(Guid packageId, CancellationToken cancellationToken);
    Task AddAsync(CommercialPackage package, CancellationToken cancellationToken);
    Task AddRangeAsync(IReadOnlyCollection<CommercialPackage> packages, CancellationToken cancellationToken);
    Task AddFeaturesAsync(IReadOnlyCollection<CommercialPackageFeature> features, CancellationToken cancellationToken);
    Task RemoveFeaturesAsync(IReadOnlyCollection<CommercialPackageFeature> features, CancellationToken cancellationToken);
}
