using System.Text.Json;
using BillingService.Domain.Common;
using BillingService.Domain.Exceptions;

namespace BillingService.Domain.Aggregates;

public sealed class TenantFeatureOverride : AggregateRoot
{
    private TenantFeatureOverride() { }

    private TenantFeatureOverride(
        Guid tenantId,
        string featureKey,
        bool granted,
        int? limitValue,
        string reason,
        string source,
        string? createdBy,
        DateTimeOffset effectiveFrom,
        DateTimeOffset? effectiveTo,
        string? metadataJson)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(featureKey))
            throw new DomainException("Feature key is required.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Reason is required.");
        if (string.IsNullOrWhiteSpace(source))
            throw new DomainException("Source is required.");
        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
            throw new DomainException("EffectiveTo cannot be earlier than EffectiveFrom.");

        Id = Guid.NewGuid();
        TenantId = tenantId;
        FeatureKey = featureKey.Trim();
        Granted = granted;
        LimitValue = limitValue;
        Reason = reason.Trim();
        Source = source.Trim();
        CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? null : createdBy.Trim();
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
    public int? LimitValue { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public string? CreatedBy { get; private set; }
    public DateTimeOffset EffectiveFrom { get; private set; }
    public DateTimeOffset? EffectiveTo { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static TenantFeatureOverride Create(
        Guid tenantId,
        string featureKey,
        bool granted,
        string reason,
        string source,
        int? limitValue = null,
        string? createdBy = null,
        DateTimeOffset? effectiveFrom = null,
        DateTimeOffset? effectiveTo = null,
        object? metadata = null)
        => new(
            tenantId,
            featureKey,
            granted,
            limitValue,
            reason,
            source,
            createdBy,
            effectiveFrom ?? DateTimeOffset.UtcNow,
            effectiveTo,
            metadata is null ? null : metadata is string rawJson ? rawJson : JsonSerializer.Serialize(metadata));

    public bool IsEffectiveAt(DateTimeOffset when)
        => EffectiveFrom <= when
           && (EffectiveTo is null || EffectiveTo >= when)
           && DeletedAt is null;

    public void Update(string featureKey, bool granted, int? limitValue, string reason, string source, string? createdBy, DateTimeOffset effectiveFrom, DateTimeOffset? effectiveTo, string? metadataJson)
    {
        if (DeletedAt is not null)
            throw new DomainException("Cannot update a deleted tenant feature override.");
        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
            throw new DomainException("EffectiveTo cannot be earlier than EffectiveFrom.");

        FeatureKey = featureKey.Trim();
        Granted = granted;
        LimitValue = limitValue;
        Reason = reason.Trim();
        Source = source.Trim();
        CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? null : createdBy.Trim();
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Delete()
    {
        if (DeletedAt is not null)
            return;

        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DeletedAt.Value;
    }
}
