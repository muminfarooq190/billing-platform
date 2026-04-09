namespace CommunicationService.Application.Abstractions;

public interface IFeatureGate
{
    Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken);
    Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken);
    Task<int?> GetLimitAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken);
}
