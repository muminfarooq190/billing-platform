using BillingService.Domain.Common;

namespace BillingService.Domain.Aggregates;

public sealed class TenantSubscriptionPackage : AggregateRoot
{
    private TenantSubscriptionPackage() { }

    private TenantSubscriptionPackage(Guid tenantId, Guid commercialPackageId, string source, string status, DateTimeOffset effectiveFrom, DateTimeOffset? effectiveTo, string? metadataJson)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        CommercialPackageId = commercialPackageId;
        Source = source;
        Status = status;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        MetadataJson = metadataJson;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid CommercialPackageId { get; private set; }
    public string Source { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public DateTimeOffset EffectiveFrom { get; private set; }
    public DateTimeOffset? EffectiveTo { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public bool IsEffectiveAt(DateTimeOffset at)
        => DeletedAt is null
           && string.Equals(Status, "Active", StringComparison.OrdinalIgnoreCase)
           && EffectiveFrom <= at
           && (!EffectiveTo.HasValue || EffectiveTo.Value >= at);

    public static TenantSubscriptionPackage Create(Guid tenantId, Guid commercialPackageId, string source, string status, DateTimeOffset effectiveFrom, DateTimeOffset? effectiveTo = null, string? metadataJson = null)
        => new(tenantId, commercialPackageId, source.Trim(), status.Trim(), effectiveFrom, effectiveTo, metadataJson);
}
