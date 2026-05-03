namespace IdentityService.Api.Contracts;

public sealed record PublicBrandingResponse(
    string? DisplayName,
    string? Tagline,
    string? PrimaryColor,
    string? SecondaryColor,
    string? AccentColor,
    string? TextColor,
    string? BackgroundColor,
    string? ThemeMode,
    string? DefaultFontFamily,
    string? SupportEmail,
    string? SupportPhone,
    string? WebsiteUrl,
    IReadOnlyList<PublicBrandingAssetResponse> Assets
);

public sealed record PublicBrandingAssetResponse(
    Guid Id,
    string AssetType,
    string OriginalFileName,
    string ContentType,
    string? AltText,
    bool IsActive,
    DateTimeOffset CreatedAt
);
