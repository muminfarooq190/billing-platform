namespace IdentityService.Api.Contracts;

public sealed record UpdateTenantTemplateThemeRequest(
    string? HeaderHtml,
    string? FooterHtml,
    string? CustomCss,
    Guid? LogoAssetId,
    Guid? BackgroundAssetId,
    string? SettingsJson);

public sealed record TenantTemplateThemeResponse(
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
