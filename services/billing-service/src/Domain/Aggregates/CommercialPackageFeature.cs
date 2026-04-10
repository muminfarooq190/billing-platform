using BillingService.Domain.Common;

namespace BillingService.Domain.Aggregates;

public sealed class CommercialPackageFeature : AggregateRoot
{
    private CommercialPackageFeature() { }

    private CommercialPackageFeature(Guid commercialPackageId, string featureKey, bool granted, int? limitValue, string? metadataJson)
    {
        Id = Guid.NewGuid();
        CommercialPackageId = commercialPackageId;
        FeatureKey = featureKey;
        Granted = granted;
        LimitValue = limitValue;
        MetadataJson = metadataJson;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid CommercialPackageId { get; private set; }
    public string FeatureKey { get; private set; } = string.Empty;
    public bool Granted { get; private set; }
    public int? LimitValue { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static CommercialPackageFeature Create(Guid commercialPackageId, string featureKey, bool granted, int? limitValue = null, string? metadataJson = null)
        => new(commercialPackageId, featureKey.Trim(), granted, limitValue, metadataJson);
}
