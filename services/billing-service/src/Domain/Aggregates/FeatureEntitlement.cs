using System.Text.Json;
using BillingService.Domain.Common;
using BillingService.Domain.Enums;
using BillingService.Domain.Exceptions;

namespace BillingService.Domain.Aggregates;

public sealed class FeatureEntitlement : AggregateRoot
{
    private FeatureEntitlement() { }

    private FeatureEntitlement(Guid tenantId, string featureKey, bool granted, EntitlementSource source, PlanType? planType, int? limitValue, DateTimeOffset effectiveFrom, DateTimeOffset? effectiveTo, string? metadataJson)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(featureKey))
            throw new DomainException("Feature key is required.");

        Id = Guid.NewGuid();
        TenantId = tenantId;
        FeatureKey = featureKey.Trim();
        Granted = granted;
        Source = source;
        PlanType = planType;
        LimitValue = limitValue;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string FeatureKey { get; private set; } = string.Empty;
    public bool Granted { get; private set; }
    public EntitlementSource Source { get; private set; }
    public PlanType? PlanType { get; private set; }
    public int? LimitValue { get; private set; }
    public DateTimeOffset EffectiveFrom { get; private set; }
    public DateTimeOffset? EffectiveTo { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static FeatureEntitlement Create(Guid tenantId, string featureKey, bool granted, EntitlementSource source, PlanType? planType = null, int? limitValue = null, DateTimeOffset? effectiveFrom = null, DateTimeOffset? effectiveTo = null, object? metadata = null)
        => new(
            tenantId,
            featureKey,
            granted,
            source,
            planType,
            limitValue,
            effectiveFrom ?? DateTimeOffset.UtcNow,
            effectiveTo,
            metadata is null ? null : JsonSerializer.Serialize(metadata));

    public bool IsEffectiveAt(DateTimeOffset when)
        => Granted
           && EffectiveFrom <= when
           && (EffectiveTo is null || EffectiveTo >= when)
           && DeletedAt is null;
}
