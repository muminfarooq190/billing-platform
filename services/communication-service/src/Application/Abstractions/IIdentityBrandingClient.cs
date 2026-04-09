namespace CommunicationService.Application.Abstractions;

public interface IIdentityBrandingClient
{
    Task<TenantBrandingDto?> GetBrandingAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<TenantTemplateThemeDto?> GetTemplateThemeAsync(Guid tenantId, string scope, CancellationToken cancellationToken);
}

public sealed record TenantBrandingDto(
    string DisplayName,
    string PrimaryColor,
    string SecondaryColor,
    string AccentColor,
    string TextColor,
    string BackgroundColor,
    string ThemeMode,
    string DefaultFontFamily,
    string? SupportEmail,
    string? SupportPhone,
    string? WebsiteUrl,
    string? Tagline);

public sealed record TenantTemplateThemeDto(
    Guid Id,
    string TemplateScope,
    string? HeaderHtml,
    string? FooterHtml,
    string? CustomCss,
    Guid? LogoAssetId,
    Guid? BackgroundAssetId,
    string? SettingsJson,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
