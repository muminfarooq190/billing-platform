namespace IdentityService.Domain.Aggregates;

public sealed class TenantBrandAsset
{
    private TenantBrandAsset() { }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string AssetType { get; private set; } = string.Empty;
    public string StorageKey { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public int? Width { get; private set; }
    public int? Height { get; private set; }
    public string? AltText { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static TenantBrandAsset Create(
        Guid tenantId,
        string assetType,
        string storageKey,
        string originalFileName,
        string contentType,
        long sizeBytes,
        int? width,
        int? height,
        string? altText)
    {
        return new TenantBrandAsset
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetType = assetType,
            StorageKey = storageKey,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            Width = width,
            Height = height,
            AltText = altText,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void SoftDelete()
    {
        IsActive = false;
        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
