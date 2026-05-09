using IdentityService.Domain.Common;

namespace IdentityService.Domain.Aggregates;

/// <summary>
/// Per-tenant email template styling — header banner, footer signature, accent palette,
/// font stack. Communication-service templates render against this when sending tenant-branded mail.
/// </summary>
public sealed class TenantEmailTemplateStyle : AggregateRoot
{
    private TenantEmailTemplateStyle() { }

    private TenantEmailTemplateStyle(Guid tenantId, string headerHtml, string footerHtml, string accentColor, string fontFamily, string customCssJson)
    {
        TenantId = tenantId;
        HeaderHtml = headerHtml;
        FooterHtml = footerHtml;
        AccentColor = accentColor;
        FontFamily = fontFamily;
        CustomCssJson = customCssJson;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid TenantId { get; private set; }
    public string HeaderHtml { get; private set; } = string.Empty;
    public string FooterHtml { get; private set; } = string.Empty;
    public string AccentColor { get; private set; } = "#041627";
    public string FontFamily { get; private set; } = "Inter, sans-serif";
    public string CustomCssJson { get; private set; } = "{}";
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static TenantEmailTemplateStyle Create(Guid tenantId, string? headerHtml, string? footerHtml, string? accentColor, string? fontFamily, string? customCssJson)
        => new(
            tenantId,
            headerHtml ?? string.Empty,
            footerHtml ?? string.Empty,
            string.IsNullOrWhiteSpace(accentColor) ? "#041627" : accentColor!.Trim(),
            string.IsNullOrWhiteSpace(fontFamily) ? "Inter, sans-serif" : fontFamily!.Trim(),
            string.IsNullOrWhiteSpace(customCssJson) ? "{}" : customCssJson!);

    public void Update(string? headerHtml, string? footerHtml, string? accentColor, string? fontFamily, string? customCssJson)
    {
        if (headerHtml is not null) HeaderHtml = headerHtml;
        if (footerHtml is not null) FooterHtml = footerHtml;
        if (!string.IsNullOrWhiteSpace(accentColor)) AccentColor = accentColor.Trim();
        if (!string.IsNullOrWhiteSpace(fontFamily)) FontFamily = fontFamily.Trim();
        if (!string.IsNullOrWhiteSpace(customCssJson)) CustomCssJson = customCssJson;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
