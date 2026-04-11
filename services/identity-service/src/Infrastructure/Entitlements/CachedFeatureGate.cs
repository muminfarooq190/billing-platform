using IdentityService.Application.Abstractions;
using IdentityService.Domain.Exceptions;

namespace IdentityService.Infrastructure.Entitlements;

public sealed class CachedFeatureGate(ICacheService cacheService, IBillingEntitlementsClient billingEntitlementsClient) : IFeatureGate
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public async Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken)
    {
        if (!await IsEnabledAsync(featureKey, tenantId, cancellationToken))
            throw new DomainException($"Feature '{featureKey}' is not enabled for tenant '{tenantId}'.");
    }

    public async Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken)
    {
        var entitlements = await GetEntitlementsAsync(tenantId, cancellationToken);
        return entitlements.Any(x => string.Equals(x.FeatureKey, featureKey, StringComparison.OrdinalIgnoreCase) && x.Granted);
    }

    public async Task<int?> GetLimitAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken)
    {
        var entitlements = await GetEntitlementsAsync(tenantId, cancellationToken);
        return entitlements.FirstOrDefault(x => string.Equals(x.FeatureKey, featureKey, StringComparison.OrdinalIgnoreCase))?.LimitValue;
    }

    private async Task<IReadOnlyList<FeatureEntitlementDto>> GetEntitlementsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var cacheKey = $"identity:entitlements:{tenantId}";
        var cached = await cacheService.GetAsync<List<FeatureEntitlementDto>>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var fresh = (await billingEntitlementsClient.GetEffectiveEntitlementsAsync(tenantId, cancellationToken)).ToList();
        await cacheService.SetAsync(cacheKey, fresh, CacheTtl, cancellationToken);
        return fresh;
    }
}
