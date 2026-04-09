namespace IdentityService.Domain.Aggregates;

public sealed class TenantTemplateTheme
{
    private TenantTemplateTheme() { }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string TemplateScope { get; private set; } = string.Empty;
    public string? HeaderHtml { get; private set; }
    public string? FooterHtml { get; private set; }
    public string? CustomCss { get; private set; }
    public Guid? LogoAssetId { get; private set; }
    public Guid? BackgroundAssetId { get; private set; }
    public string? SettingsJson { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static TenantTemplateTheme Create(Guid tenantId, string templateScope)
    {
        return new TenantTemplateTheme
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TemplateScope = templateScope.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(string? headerHtml, string? footerHtml, string? customCss, Guid? logoAssetId, Guid? backgroundAssetId, string? settingsJson)
    {
        HeaderHtml = headerHtml;
        FooterHtml = footerHtml;
        CustomCss = customCss;
        LogoAssetId = logoAssetId;
        BackgroundAssetId = backgroundAssetId;
        SettingsJson = settingsJson;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
