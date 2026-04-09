namespace IdentityService.Api.Contracts;

public sealed record UpdateTenantBrandingRequest(
    string DisplayName,
    string? LegalName,
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
