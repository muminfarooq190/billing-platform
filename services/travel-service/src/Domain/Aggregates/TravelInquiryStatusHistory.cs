namespace TravelService.Domain.Aggregates;

public sealed class TravelInquiryStatusHistory
{
    private TravelInquiryStatusHistory() { }

    private TravelInquiryStatusHistory(Guid travelInquiryId, Guid tenantId, string? fromStatus, string toStatus, string? reason, Guid? changedByUserId)
    {
        Id = Guid.NewGuid();
        TravelInquiryId = travelInquiryId;
        TenantId = tenantId;
        FromStatus = string.IsNullOrWhiteSpace(fromStatus) ? null : fromStatus.Trim();
        ToStatus = toStatus.Trim();
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        ChangedByUserId = changedByUserId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TravelInquiryId { get; private set; }
    public Guid TenantId { get; private set; }
    public string? FromStatus { get; private set; }
    public string ToStatus { get; private set; } = string.Empty;
    public string? Reason { get; private set; }
    public Guid? ChangedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static TravelInquiryStatusHistory Create(Guid travelInquiryId, Guid tenantId, string? fromStatus, string toStatus, string? reason, Guid? changedByUserId = null)
        => new(travelInquiryId, tenantId, fromStatus, toStatus, reason, changedByUserId);
}
