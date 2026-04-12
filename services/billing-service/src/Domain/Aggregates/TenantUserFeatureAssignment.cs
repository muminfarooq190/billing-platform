using BillingService.Domain.Common;
using BillingService.Domain.Enums;
using BillingService.Domain.Exceptions;

namespace BillingService.Domain.Aggregates;

public sealed class TenantUserFeatureAssignment : AggregateRoot
{
    private TenantUserFeatureAssignment() { }

    private TenantUserFeatureAssignment(
        Guid tenantId,
        Guid userId,
        string featureKey,
        Guid? assignedByUserId,
        DateTimeOffset effectiveFrom,
        DateTimeOffset? effectiveTo,
        string? notes,
        string? metadataJson)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (userId == Guid.Empty)
            throw new DomainException("UserId is required.");
        if (string.IsNullOrWhiteSpace(featureKey))
            throw new DomainException("FeatureKey is required.");
        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
            throw new DomainException("EffectiveTo cannot be earlier than EffectiveFrom.");

        Id = Guid.NewGuid();
        TenantId = tenantId;
        UserId = userId;
        FeatureKey = featureKey.Trim();
        AssignedByUserId = assignedByUserId;
        AssignedAt = DateTimeOffset.UtcNow;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson;
        Status = "Active";
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string FeatureKey { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public Guid? AssignedByUserId { get; private set; }
    public DateTimeOffset AssignedAt { get; private set; }
    public Guid? RevokedByUserId { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public DateTimeOffset EffectiveFrom { get; private set; }
    public DateTimeOffset? EffectiveTo { get; private set; }
    public string? Notes { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static TenantUserFeatureAssignment Create(
        Guid tenantId,
        Guid userId,
        string featureKey,
        Guid? assignedByUserId,
        DateTimeOffset effectiveFrom,
        DateTimeOffset? effectiveTo,
        string? notes,
        string? metadataJson)
        => new(tenantId, userId, featureKey, assignedByUserId, effectiveFrom, effectiveTo, notes, metadataJson);

    public bool IsEffectiveAt(DateTimeOffset when)
        => DeletedAt is null
           && string.Equals(Status, "Active", StringComparison.OrdinalIgnoreCase)
           && EffectiveFrom <= when
           && (EffectiveTo is null || EffectiveTo >= when);

    public void Revoke(Guid? revokedByUserId, DateTimeOffset revokedAt)
    {
        if (DeletedAt is not null)
            throw new DomainException("Cannot revoke a deleted assignment.");
        if (!string.Equals(Status, "Active", StringComparison.OrdinalIgnoreCase))
            return;

        Status = "Revoked";
        RevokedByUserId = revokedByUserId;
        RevokedAt = revokedAt;
        UpdatedAt = revokedAt;
    }
}
