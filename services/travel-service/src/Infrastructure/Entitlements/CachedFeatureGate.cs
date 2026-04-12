using TravelService.Application.Abstractions;
using TravelService.Domain.Exceptions;

namespace TravelService.Infrastructure.Entitlements;

public sealed class CachedFeatureGate(ICacheService cacheService, IBillingEntitlementsClient billingEntitlementsClient) : IFeatureGate
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken)
        => EnsureEnabledAsync(featureKey, tenantId, null, cancellationToken);

    public async Task EnsureEnabledAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken)
    {
        if (!await IsEnabledAsync(featureKey, tenantId, userId, cancellationToken))
            throw new DomainException($"Feature '{featureKey}' is not enabled for tenant '{tenantId}' and user '{userId}'.");
    }

    public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken)
        => IsEnabledAsync(featureKey, tenantId, null, cancellationToken);

    public async Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken)
    {
        if (userId.HasValue)
        {
            var access = await GetUserFeatureAccessAsync(tenantId, userId.Value, cancellationToken);
            return access.Any(x => string.Equals(x.FeatureKey, featureKey, StringComparison.OrdinalIgnoreCase) && x.Granted);
        }

        var entitlements = await GetEntitlementsAsync(tenantId, cancellationToken);
        return entitlements.Any(x => string.Equals(x.FeatureKey, featureKey, StringComparison.OrdinalIgnoreCase) && x.Granted);
    }

    public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken)
        => GetLimitAsync(featureKey, tenantId, null, cancellationToken);

    public async Task<int?> GetLimitAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken)
    {
        if (userId.HasValue)
        {
            var access = await GetUserFeatureAccessAsync(tenantId, userId.Value, cancellationToken);
            return access.FirstOrDefault(x => string.Equals(x.FeatureKey, featureKey, StringComparison.OrdinalIgnoreCase))?.LimitValue;
        }

        var entitlements = await GetEntitlementsAsync(tenantId, cancellationToken);
        return entitlements.FirstOrDefault(x => string.Equals(x.FeatureKey, featureKey, StringComparison.OrdinalIgnoreCase))?.LimitValue;
    }

    private async Task<IReadOnlyList<FeatureEntitlementDto>> GetEntitlementsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var cacheKey = $"entitlements:{tenantId}";
        var cached = await cacheService.GetAsync<List<FeatureEntitlementDto>>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var fresh = (await billingEntitlementsClient.GetEffectiveEntitlementsAsync(tenantId, cancellationToken)).ToList();
        await cacheService.SetAsync(cacheKey, fresh, CacheTtl, cancellationToken);
        return fresh;
    }

    private async Task<IReadOnlyList<UserFeatureAccessDto>> GetUserFeatureAccessAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken)
    {
        var cacheKey = $"entitlements:{tenantId}:user:{userId}";
        var cached = await cacheService.GetAsync<List<UserFeatureAccessDto>>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var fresh = (await billingEntitlementsClient.GetUserFeatureAccessAsync(tenantId, userId, cancellationToken)).ToList();
        await cacheService.SetAsync(cacheKey, fresh, CacheTtl, cancellationToken);
        return fresh;
    }
}
