using CommunicationService.Application.Abstractions;

namespace CommunicationService.Infrastructure.Branding;

public sealed class DefaultBrandingTemplateRenderer : IBrandingTemplateRenderer
{
    public Dictionary<string, string> Enrich(Guid tenantId, Dictionary<string, string> placeholders)
    {
        var merged = new Dictionary<string, string>(placeholders, StringComparer.OrdinalIgnoreCase)
        {
            ["BrandDisplayName"] = placeholders.GetValueOrDefault("BrandDisplayName") ?? "Voyara",
            ["BrandPrimaryColor"] = placeholders.GetValueOrDefault("BrandPrimaryColor") ?? "#2563eb",
            ["BrandSupportEmail"] = placeholders.GetValueOrDefault("BrandSupportEmail") ?? "support@voyara.local"
        };

        return merged;
    }
}
