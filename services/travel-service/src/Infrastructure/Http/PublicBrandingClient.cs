using System.Net;
using System.Net.Http.Json;
using TravelService.Application.Abstractions;

namespace TravelService.Infrastructure.Http;

public sealed class PublicBrandingClient(HttpClient httpClient) : IPublicBrandingClient
{
    public async Task<PublicBrandingDto?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"identity/internal/public-branding/{tenantId}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PublicBrandingDto>(cancellationToken: cancellationToken);
    }
}
