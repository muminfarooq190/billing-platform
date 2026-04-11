using BillingService.Domain.Common;
using BillingService.Domain.Exceptions;

namespace BillingService.Domain.Aggregates;

public sealed class FeatureCatalogEntry : AggregateRoot
{
    private FeatureCatalogEntry() { }

    private FeatureCatalogEntry(string featureKey, string service, string category, string displayName, string description, bool isQuota, string? unit, string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(featureKey))
            throw new DomainException("Feature key is required.");
        if (string.IsNullOrWhiteSpace(service))
            throw new DomainException("Service is required.");
        if (string.IsNullOrWhiteSpace(category))
            throw new DomainException("Category is required.");
        if (string.IsNullOrWhiteSpace(displayName))
            throw new DomainException("Display name is required.");
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required.");

        Id = Guid.NewGuid();
        FeatureKey = featureKey.Trim();
        Service = service.Trim();
        Category = category.Trim();
        DisplayName = displayName.Trim();
        Description = description.Trim();
        IsQuota = isQuota;
        Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim();
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson;
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
        => new(featureKey, service, category, displayName, description, isQuota, unit, metadataJson);

    public void Update(string service, string category, string displayName, string description, bool isQuota, string? unit, string? metadataJson)
    {
        if (DeletedAt is not null)
            throw new DomainException("Cannot update a deleted feature catalog entry.");

        Service = service.Trim();
        Category = category.Trim();
        DisplayName = displayName.Trim();
        Description = description.Trim();
        IsQuota = isQuota;
        Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim();
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
