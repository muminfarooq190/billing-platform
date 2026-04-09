using CommunicationService.Application.Abstractions;

namespace CommunicationService.Infrastructure.Branding;

public sealed class IdentityBackedBrandingTemplateRenderer(IIdentityBrandingClient identityBrandingClient) : IBrandingTemplateRenderer
{
    public async Task<Dictionary<string, string>> EnrichAsync(Guid tenantId, string scope, Dictionary<string, string> placeholders, CancellationToken cancellationToken)
    {
        var merged = new Dictionary<string, string>(placeholders, StringComparer.OrdinalIgnoreCase);
        var branding = await identityBrandingClient.GetBrandingAsync(tenantId, cancellationToken);
        var theme = await identityBrandingClient.GetTemplateThemeAsync(tenantId, scope, cancellationToken);

        merged["BrandDisplayName"] = placeholders.GetValueOrDefault("BrandDisplayName") ?? branding?.DisplayName ?? "Voyara";
        merged["BrandPrimaryColor"] = placeholders.GetValueOrDefault("BrandPrimaryColor") ?? branding?.PrimaryColor ?? "#2563eb";
        merged["BrandSupportEmail"] = placeholders.GetValueOrDefault("BrandSupportEmail") ?? branding?.SupportEmail ?? "support@voyara.local";
        if (!string.IsNullOrWhiteSpace(theme?.HeaderHtml))
            merged["BrandHeaderHtml"] = theme.HeaderHtml;
        if (!string.IsNullOrWhiteSpace(theme?.FooterHtml))
            merged["BrandFooterHtml"] = theme.FooterHtml;
        if (!string.IsNullOrWhiteSpace(theme?.CustomCss))
            merged["BrandCustomCss"] = theme.CustomCss;

        return merged;
    }
}
