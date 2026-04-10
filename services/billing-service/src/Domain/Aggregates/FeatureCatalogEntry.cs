using BillingService.Domain.Common;

namespace BillingService.Domain.Aggregates;

public sealed class FeatureCatalogEntry : AggregateRoot
{
    private FeatureCatalogEntry() { }

    private FeatureCatalogEntry(string featureKey, string service, string category, string displayName, string description, bool isQuota, string? unit, string? metadataJson)
    {
        Id = Guid.NewGuid();
        FeatureKey = featureKey;
        Service = service;
        Category = category;
        DisplayName = displayName;
        Description = description;
        IsQuota = isQuota;
        Unit = unit;
        MetadataJson = metadataJson;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string FeatureKey { get; private set; } = string.Empty;
    public string Service { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsQuota { get; private set; }
    public string? Unit { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static FeatureCatalogEntry Create(string featureKey, string service, string category, string displayName, string description, bool isQuota = false, string? unit = null, string? metadataJson = null)
        => new(featureKey.Trim(), service.Trim(), category.Trim(), displayName.Trim(), description.Trim(), isQuota, unit, metadataJson);
}
