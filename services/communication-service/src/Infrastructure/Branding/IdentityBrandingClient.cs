using System.Net.Http.Json;
using CommunicationService.Application.Abstractions;

namespace CommunicationService.Infrastructure.Branding;

public sealed class IdentityBrandingClient(HttpClient httpClient) : IIdentityBrandingClient
{
    public async Task<TenantBrandingDto?> GetBrandingAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "tenant-branding");
        request.Headers.Add("X-Tenant-Id", tenantId.ToString());
        var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TenantBrandingDto>(cancellationToken: cancellationToken);
    }

    public async Task<TenantTemplateThemeDto?> GetTemplateThemeAsync(Guid tenantId, string scope, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"tenant-branding/templates/{scope}");
        request.Headers.Add("X-Tenant-Id", tenantId.ToString());
        var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TenantTemplateThemeDto>(cancellationToken: cancellationToken);
    }
}
