namespace TravelService.Application.Abstractions;

public interface IPublicBrandingClient
{
    Task<PublicBrandingDto?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken);
}

public sealed record PublicBrandingDto(
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
    IReadOnlyList<PublicBrandingAssetDto> Assets
);

public sealed record PublicBrandingAssetDto(
    Guid Id,
    string AssetType,
    string OriginalFileName,
    string ContentType,
    string? AltText,
    bool IsActive,
    DateTimeOffset CreatedAt
);
