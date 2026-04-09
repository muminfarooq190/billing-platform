namespace IdentityService.Api.Contracts;

public sealed record TenantBrandingAssetResponse(
    Guid Id,
    string AssetType,
    string StorageKey,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    int? Width,
    int? Height,
    string? AltText,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
