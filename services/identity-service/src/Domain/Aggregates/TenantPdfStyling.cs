using IdentityService.Domain.Common;

namespace IdentityService.Domain.Aggregates;

/// <summary>
/// Per-tenant PDF appearance settings (header layout, footer text, watermark, margins, accent color).
/// Renderers (quotation, itinerary, invoice) read this to style output. One row per tenant.
/// </summary>
public sealed class TenantPdfStyling : AggregateRoot
{
    private TenantPdfStyling() { }

    private TenantPdfStyling(Guid tenantId, string headerLayout, string footerText, string watermarkText, string accentColor, int marginPx, string customCssJson)
    {
        TenantId = tenantId;
        HeaderLayout = headerLayout;
        FooterText = footerText;
        WatermarkText = watermarkText;
        AccentColor = accentColor;
        MarginPx = marginPx;
        CustomCssJson = customCssJson;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid TenantId { get; private set; }
    public string HeaderLayout { get; private set; } = "logo-left";
    public string FooterText { get; private set; } = string.Empty;
    public string WatermarkText { get; private set; } = string.Empty;
    public string AccentColor { get; private set; } = "#041627";
    public int MarginPx { get; private set; } = 40;
    public string CustomCssJson { get; private set; } = "{}";
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static TenantPdfStyling Create(Guid tenantId, string? headerLayout, string? footerText, string? watermarkText, string? accentColor, int? marginPx, string? customCssJson)
        => new(
            tenantId,
            string.IsNullOrWhiteSpace(headerLayout) ? "logo-left" : headerLayout!.Trim(),
            footerText?.Trim() ?? string.Empty,
            watermarkText?.Trim() ?? string.Empty,
            string.IsNullOrWhiteSpace(accentColor) ? "#041627" : accentColor!.Trim(),
            marginPx is { } m && m >= 0 ? m : 40,
            string.IsNullOrWhiteSpace(customCssJson) ? "{}" : customCssJson!);

    public void Update(string? headerLayout, string? footerText, string? watermarkText, string? accentColor, int? marginPx, string? customCssJson)
    {
        if (!string.IsNullOrWhiteSpace(headerLayout)) HeaderLayout = headerLayout.Trim();
        FooterText = footerText?.Trim() ?? FooterText;
        WatermarkText = watermarkText?.Trim() ?? WatermarkText;
        if (!string.IsNullOrWhiteSpace(accentColor)) AccentColor = accentColor.Trim();
        if (marginPx is { } m && m >= 0) MarginPx = m;
        if (!string.IsNullOrWhiteSpace(customCssJson)) CustomCssJson = customCssJson;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
