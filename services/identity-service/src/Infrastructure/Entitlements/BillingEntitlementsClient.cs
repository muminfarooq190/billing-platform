using System.Net.Http.Json;
using IdentityService.Application.Abstractions;

namespace IdentityService.Infrastructure.Entitlements;

public sealed class BillingEntitlementsClient(HttpClient httpClient) : IBillingEntitlementsClient
{
    public async Task<IReadOnlyList<FeatureEntitlementDto>> GetEffectiveEntitlementsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var result = await httpClient.GetFromJsonAsync<IReadOnlyList<FeatureEntitlementDto>>($"billing/entitlements/{tenantId}", cancellationToken);
        return result ?? [];
    }

    public async Task<IReadOnlyList<UserFeatureAccessDto>> GetUserFeatureAccessAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken)
    {
        var result = await httpClient.GetFromJsonAsync<IReadOnlyList<UserFeatureAccessDto>>($"billing/tenants/{tenantId}/users/{userId}/features", cancellationToken);
        return result ?? [];
    }
}
