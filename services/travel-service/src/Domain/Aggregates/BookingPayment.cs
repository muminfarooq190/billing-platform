using TravelService.Domain.Common;
using TravelService.Domain.Enums;
using TravelService.Domain.Exceptions;

namespace TravelService.Domain.Aggregates;

public sealed class BookingPayment : AggregateRoot
{
    private BookingPayment() { }

    private BookingPayment(Guid tenantId, Guid bookingId, string? milestoneLabel, DateTimeOffset dueDate, decimal amount, string currency, Guid? recordedByUserId, string? notes)
    {
        if (tenantId == Guid.Empty) throw new DomainException("TenantId is required.");
        if (bookingId == Guid.Empty) throw new DomainException("BookingId is required.");
        if (amount < 0) throw new DomainException("Payment amount cannot be negative.");
        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3) throw new DomainException("Currency must be a 3-letter ISO code.");

        Id = Guid.NewGuid();
        TenantId = tenantId;
        BookingId = bookingId;
        MilestoneLabel = NormalizeOptional(milestoneLabel);
        DueDate = dueDate;
        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
        Status = BookingPaymentStatus.Scheduled;
        RecordedByUserId = recordedByUserId;
        Notes = NormalizeOptional(notes);
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid BookingId { get; private set; }
    public string? MilestoneLabel { get; private set; }
    public DateTimeOffset DueDate { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public BookingPaymentStatus Status { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public string? PaymentMethod { get; private set; }
    public string? ProviderReference { get; private set; }
    public Guid? RecordedByUserId { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static BookingPayment Schedule(Guid tenantId, Guid bookingId, string? milestoneLabel, DateTimeOffset dueDate, decimal amount, string currency, Guid? recordedByUserId, string? notes = null)
        => new(tenantId, bookingId, milestoneLabel, dueDate, amount, currency, recordedByUserId, notes);

    public void Update(string? milestoneLabel, DateTimeOffset dueDate, decimal amount, string currency, string? notes)
    {
        if (Status != BookingPaymentStatus.Scheduled)
            throw new DomainException("Only scheduled payments can be edited.");
        if (amount < 0)
            throw new DomainException("Payment amount cannot be negative.");
        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw new DomainException("Currency must be a 3-letter ISO code.");

        MilestoneLabel = NormalizeOptional(milestoneLabel);
        DueDate = dueDate;
        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
        Notes = NormalizeOptional(notes);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkPaid(DateTimeOffset? paidAt, string paymentMethod, string? providerReference, string? notes, Guid? recordedByUserId)
    {
        if (Status is BookingPaymentStatus.Paid or BookingPaymentStatus.Refunded or BookingPaymentStatus.Waived)
            throw new DomainException("Payment cannot be marked paid from its current status.");
        if (string.IsNullOrWhiteSpace(paymentMethod))
            throw new DomainException("Payment method is required.");

        Status = BookingPaymentStatus.Paid;
        PaidAt = paidAt ?? DateTimeOffset.UtcNow;
        PaymentMethod = paymentMethod.Trim();
        ProviderReference = NormalizeOptional(providerReference);
        RecordedByUserId = recordedByUserId;
        Notes = MergeNotes(notes);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Refund(string? notes, Guid? recordedByUserId)
    {
        if (Status != BookingPaymentStatus.Paid)
            throw new DomainException("Only paid payments can be refunded.");

        Status = BookingPaymentStatus.Refunded;
        RecordedByUserId = recordedByUserId;
        Notes = MergeNotes(notes);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Waive(string? notes, Guid? recordedByUserId)
    {
        if (Status == BookingPaymentStatus.Paid)
            throw new DomainException("Paid payments cannot be waived.");
        if (Status == BookingPaymentStatus.Refunded)
            throw new DomainException("Refunded payments cannot be waived.");

        Status = BookingPaymentStatus.Waived;
        RecordedByUserId = recordedByUserId;
        Notes = MergeNotes(notes);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete()
    {
        if (Status != BookingPaymentStatus.Scheduled)
            throw new DomainException("Only scheduled payments can be deleted.");

        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private string? MergeNotes(string? notes)
    {
        var normalized = NormalizeOptional(notes);
        if (string.IsNullOrWhiteSpace(normalized))
            return Notes;
        if (string.IsNullOrWhiteSpace(Notes))
            return normalized;
        return $"{Notes}{Environment.NewLine}{normalized}";
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
