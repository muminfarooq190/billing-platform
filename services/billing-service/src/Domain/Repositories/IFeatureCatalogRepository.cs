using BillingService.Domain.Aggregates;

namespace BillingService.Domain.Repositories;

public interface IFeatureCatalogRepository
{
    Task<IReadOnlyList<FeatureCatalogEntry>> ListAsync(CancellationToken cancellationToken);
    Task<FeatureCatalogEntry?> GetByFeatureKeyAsync(string featureKey, CancellationToken cancellationToken);
    Task AddAsync(FeatureCatalogEntry entry, CancellationToken cancellationToken);
}
