namespace IdentityService.Infrastructure.Entitlements;

public interface IBillingEntitlementsClient
{
    Task<IReadOnlyList<FeatureEntitlementDto>> GetEffectiveEntitlementsAsync(Guid tenantId, CancellationToken cancellationToken);
}
