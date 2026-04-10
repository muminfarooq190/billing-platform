using BillingService.Domain.Aggregates;

namespace BillingService.Domain.Repositories;

public interface ICommercialPackageRepository
{
    Task<IReadOnlyList<CommercialPackage>> ListActiveAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<CommercialPackageFeature>> ListFeaturesByPackageIdsAsync(IReadOnlyCollection<Guid> packageIds, CancellationToken cancellationToken);
    Task AddRangeAsync(IReadOnlyCollection<CommercialPackage> packages, CancellationToken cancellationToken);
    Task AddFeaturesAsync(IReadOnlyCollection<CommercialPackageFeature> features, CancellationToken cancellationToken);
}
