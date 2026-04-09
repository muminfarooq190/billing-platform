namespace IdentityService.Domain.Aggregates;

public sealed class TenantBranding
{
    private TenantBranding() { }

    public Guid TenantId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string? LegalName { get; private set; }
    public string PrimaryColor { get; private set; } = "#0F172A";
    public string SecondaryColor { get; private set; } = "#1D4ED8";
    public string AccentColor { get; private set; } = "#F59E0B";
    public string TextColor { get; private set; } = "#111827";
    public string BackgroundColor { get; private set; } = "#FFFFFF";
    public string ThemeMode { get; private set; } = "Light";
    public string DefaultFontFamily { get; private set; } = "Inter";
    public string? SupportEmail { get; private set; }
    public string? SupportPhone { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public string? Tagline { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static TenantBranding Create(Guid tenantId, string displayName)
    {
        return new TenantBranding
        {
            TenantId = tenantId,
            DisplayName = displayName,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(
        string displayName,
        string? legalName,
        string primaryColor,
        string secondaryColor,
        string accentColor,
        string textColor,
        string backgroundColor,
        string themeMode,
        string defaultFontFamily,
        string? supportEmail,
        string? supportPhone,
        string? websiteUrl,
        string? tagline)
    {
        DisplayName = displayName;
        LegalName = legalName;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        AccentColor = accentColor;
        TextColor = textColor;
        BackgroundColor = backgroundColor;
        ThemeMode = themeMode;
        DefaultFontFamily = defaultFontFamily;
        SupportEmail = supportEmail;
        SupportPhone = supportPhone;
        WebsiteUrl = websiteUrl;
        Tagline = tagline;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
