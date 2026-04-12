using System.Net.Http.Json;
using CommunicationService.Application.Abstractions;

namespace CommunicationService.Infrastructure.Entitlements;

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
