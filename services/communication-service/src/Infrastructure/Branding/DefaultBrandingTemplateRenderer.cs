using CommunicationService.Application.Abstractions;

namespace CommunicationService.Infrastructure.Branding;

public sealed class DefaultBrandingTemplateRenderer : IBrandingTemplateRenderer
{
    public Task<Dictionary<string, string>> EnrichAsync(Guid tenantId, string scope, Dictionary<string, string> placeholders, CancellationToken cancellationToken)
    {
        var merged = new Dictionary<string, string>(placeholders, StringComparer.OrdinalIgnoreCase)
        {
            ["BrandDisplayName"] = placeholders.GetValueOrDefault("BrandDisplayName") ?? "Voyara",
            ["BrandPrimaryColor"] = placeholders.GetValueOrDefault("BrandPrimaryColor") ?? "#2563eb",
            ["BrandSupportEmail"] = placeholders.GetValueOrDefault("BrandSupportEmail") ?? "support@voyara.local"
        };

        return Task.FromResult(merged);
    }
}
