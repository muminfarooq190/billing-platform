using BillingService.Domain.Common;
using BillingService.Domain.Exceptions;

namespace BillingService.Domain.Aggregates;

public sealed class TenantSubscriptionPackage : AggregateRoot
{
    private TenantSubscriptionPackage() { }

    private TenantSubscriptionPackage(Guid tenantId, Guid commercialPackageId, string source, string status, DateTimeOffset effectiveFrom, DateTimeOffset? effectiveTo, string? metadataJson)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (commercialPackageId == Guid.Empty)
            throw new DomainException("CommercialPackageId is required.");
        if (string.IsNullOrWhiteSpace(source))
            throw new DomainException("Source is required.");
        if (string.IsNullOrWhiteSpace(status))
            throw new DomainException("Status is required.");
        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
            throw new DomainException("EffectiveTo cannot be earlier than EffectiveFrom.");

        Id = Guid.NewGuid();
        TenantId = tenantId;
        CommercialPackageId = commercialPackageId;
        Source = source.Trim();
        Status = status.Trim();
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson;
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
        => new(tenantId, commercialPackageId, source, status, effectiveFrom, effectiveTo, metadataJson);

    public void Update(Guid commercialPackageId, string source, string status, DateTimeOffset effectiveFrom, DateTimeOffset? effectiveTo, string? metadataJson)
    {
        if (DeletedAt is not null)
            throw new DomainException("Cannot update a deleted tenant package assignment.");
        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
            throw new DomainException("EffectiveTo cannot be earlier than EffectiveFrom.");

        CommercialPackageId = commercialPackageId;
        Source = source.Trim();
        Status = status.Trim();
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
