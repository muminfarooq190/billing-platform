using TravelService.Domain.Exceptions;

namespace TravelService.Domain.Aggregates;

public sealed class BookingChangeRequest
{
    private BookingChangeRequest() { }

    private BookingChangeRequest(Guid bookingId, Guid tenantId, string changeType, string reason, Guid? requestedByUserId)
    {
        if (bookingId == Guid.Empty) throw new DomainException("BookingId is required.");
        if (tenantId == Guid.Empty) throw new DomainException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(changeType)) throw new DomainException("Change type is required.");
        if (string.IsNullOrWhiteSpace(reason)) throw new DomainException("Reason is required.");

        Id = Guid.NewGuid();
        BookingId = bookingId;
        TenantId = tenantId;
        ChangeType = changeType.Trim();
        Reason = reason.Trim();
        Status = "Pending";
        RequestedByUserId = requestedByUserId;
        RequestedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid BookingId { get; private set; }
    public Guid TenantId { get; private set; }
    public string ChangeType { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public Guid? RequestedByUserId { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public string? DecisionReason { get; private set; }
    public DateTimeOffset RequestedAt { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static BookingChangeRequest Create(Guid bookingId, Guid tenantId, string changeType, string reason, Guid? requestedByUserId = null)
        => new(bookingId, tenantId, changeType, reason, requestedByUserId);

    public void Approve(Guid? reviewedByUserId, string? decisionReason)
    {
        Status = "Approved";
        ReviewedByUserId = reviewedByUserId;
        DecisionReason = string.IsNullOrWhiteSpace(decisionReason) ? null : decisionReason.Trim();
        ReviewedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(Guid? reviewedByUserId, string? decisionReason)
    {
        Status = "Rejected";
        ReviewedByUserId = reviewedByUserId;
        DecisionReason = string.IsNullOrWhiteSpace(decisionReason) ? null : decisionReason.Trim();
        ReviewedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
