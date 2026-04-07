using TravelService.Domain.Exceptions;

namespace TravelService.Domain.Aggregates;

public sealed class BookingStatusHistory
{
    private BookingStatusHistory() { }

    private BookingStatusHistory(Guid bookingId, Guid tenantId, string? fromStatus, string toStatus, string? reason, Guid? changedByUserId)
    {
        if (bookingId == Guid.Empty)
            throw new DomainException("BookingId is required.");
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(toStatus))
            throw new DomainException("ToStatus is required.");

        Id = Guid.NewGuid();
        BookingId = bookingId;
        TenantId = tenantId;
        FromStatus = string.IsNullOrWhiteSpace(fromStatus) ? null : fromStatus.Trim();
        ToStatus = toStatus.Trim();
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        ChangedByUserId = changedByUserId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid BookingId { get; private set; }
    public Guid TenantId { get; private set; }
    public string? FromStatus { get; private set; }
    public string ToStatus { get; private set; } = string.Empty;
    public string? Reason { get; private set; }
    public Guid? ChangedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static BookingStatusHistory Create(Guid bookingId, Guid tenantId, string? fromStatus, string toStatus, string? reason = null, Guid? changedByUserId = null)
        => new(bookingId, tenantId, fromStatus, toStatus, reason, changedByUserId);
}
