namespace IdentityService.Infrastructure.Entitlements;

public interface IBillingEntitlementsClient
{
    Task<IReadOnlyList<FeatureEntitlementDto>> GetEffectiveEntitlementsAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserFeatureAccessDto>> GetUserFeatureAccessAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken);
}
