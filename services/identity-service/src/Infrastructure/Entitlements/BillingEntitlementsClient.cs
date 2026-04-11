using System.Net.Http.Json;

namespace IdentityService.Infrastructure.Entitlements;

public sealed class BillingEntitlementsClient(HttpClient httpClient) : IBillingEntitlementsClient
{
    public async Task<IReadOnlyList<FeatureEntitlementDto>> GetEffectiveEntitlementsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var result = await httpClient.GetFromJsonAsync<IReadOnlyList<FeatureEntitlementDto>>($"billing/entitlements/{tenantId}", cancellationToken);
        return result ?? [];
    }
}
