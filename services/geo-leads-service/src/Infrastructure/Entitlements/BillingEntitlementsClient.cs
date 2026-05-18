using System.Net.Http.Json;
using GeoLeadsService.Application.Abstractions;

namespace GeoLeadsService.Infrastructure.Entitlements;

public sealed class BillingEntitlementsClient(HttpClient httpClient) : IBillingEntitlementsClient
{
    public async Task<IReadOnlyList<FeatureEntitlementDto>> GetEffectiveEntitlementsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        // billing-service requires `x-tenant-id` to resolve ITenantContext.
        // Without it, the controller throws and we cascade as a 500 from the
        // caller's FeatureGate. Forward the tenant id explicitly here.
        using var request = new HttpRequestMessage(HttpMethod.Get, $"billing/entitlements/{tenantId}");
        request.Headers.TryAddWithoutValidation("x-tenant-id", tenantId.ToString());
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return Array.Empty<FeatureEntitlementDto>();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<FeatureEntitlementDto>>(cancellationToken: cancellationToken) ?? Array.Empty<FeatureEntitlementDto>();
    }
}
