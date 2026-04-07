using TravelService.Domain.Common;
using TravelService.Domain.Enums;
using TravelService.Domain.Exceptions;

namespace TravelService.Domain.Aggregates;

public sealed class Booking : AggregateRoot
{
    private Booking() { }

    private Booking(Guid tenantId, Guid quotationId, Guid acceptedRevisionId, Guid primaryContactId, string bookingNumber, string tripName, string destination, DateTimeOffset startDate, DateTimeOffset endDate, int travellersCount, string currency, decimal totalSellAmount, Guid? assignedToUserId, string? customerReference, string? internalNotes)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (quotationId == Guid.Empty)
            throw new DomainException("QuotationId is required.");
        if (acceptedRevisionId == Guid.Empty)
            throw new DomainException("AcceptedRevisionId is required.");
        if (primaryContactId == Guid.Empty)
            throw new DomainException("PrimaryContactId is required.");
        if (string.IsNullOrWhiteSpace(bookingNumber))
            throw new DomainException("Booking number is required.");
        if (string.IsNullOrWhiteSpace(tripName))
            throw new DomainException("Trip name is required.");
        if (string.IsNullOrWhiteSpace(destination))
            throw new DomainException("Destination is required.");
        if (travellersCount <= 0)
            throw new DomainException("Travellers count must be greater than zero.");
        if (endDate < startDate)
            throw new DomainException("End date must be on or after start date.");
        if (totalSellAmount < 0)
            throw new DomainException("Total sell amount cannot be negative.");

        Id = Guid.NewGuid();
        TenantId = tenantId;
        QuotationId = quotationId;
        AcceptedRevisionId = acceptedRevisionId;
        PrimaryContactId = primaryContactId;
        BookingNumber = bookingNumber.Trim();
        Status = BookingStatus.Pending;
        TripName = tripName.Trim();
        Destination = destination.Trim();
        StartDate = startDate;
        EndDate = endDate;
        TravellersCount = travellersCount;
        Currency = NormalizeCurrency(currency);
        TotalSellAmount = totalSellAmount;
        AssignedToUserId = assignedToUserId;
        CustomerReference = NormalizeOptional(customerReference);
        InternalNotes = NormalizeOptional(internalNotes);
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid? QuotationId { get; private set; }
    public Guid? AcceptedRevisionId { get; private set; }
    public Guid PrimaryContactId { get; private set; }
    public string BookingNumber { get; private set; } = string.Empty;
    public BookingStatus Status { get; private set; }
    public string TripName { get; private set; } = string.Empty;
    public string Destination { get; private set; } = string.Empty;
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public int TravellersCount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public decimal TotalSellAmount { get; private set; }
    public decimal? TotalCostAmount { get; private set; }
    public decimal? MarginAmount { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public string? CustomerReference { get; private set; }
    public string? InternalNotes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static Booking CreateFromAcceptedQuotation(Guid tenantId, Guid quotationId, Guid acceptedRevisionId, Guid primaryContactId, string bookingNumber, string tripName, string destination, DateTimeOffset startDate, DateTimeOffset endDate, int travellersCount, string currency, decimal totalSellAmount, Guid? assignedToUserId = null, string? customerReference = null, string? internalNotes = null)
        => new(tenantId, quotationId, acceptedRevisionId, primaryContactId, bookingNumber, tripName, destination, startDate, endDate, travellersCount, currency, totalSellAmount, assignedToUserId, customerReference, internalNotes);

    public void UpdateStatus(BookingStatus status)
    {
        if (Status == BookingStatus.Cancelled)
            throw new DomainException("Cancelled bookings cannot transition to another status.");
        if (Status == BookingStatus.Completed && status != BookingStatus.Completed)
            throw new DomainException("Completed bookings cannot move back to another status.");
        if (status == BookingStatus.Cancelled)
            throw new DomainException("Use Cancel for cancelled state transitions.");

        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel(string? reason = null)
    {
        if (Status == BookingStatus.Completed)
            throw new DomainException("Completed bookings cannot be cancelled.");
        if (Status == BookingStatus.Cancelled)
            throw new DomainException("Booking is already cancelled.");

        Status = BookingStatus.Cancelled;
        CancelledAt = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(reason))
            InternalNotes = string.Join(Environment.NewLine, new[] { InternalNotes, $"Cancellation reason: {reason.Trim()}" }.Where(x => !string.IsNullOrWhiteSpace(x)));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizeCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency is required.");

        var normalized = currency.Trim().ToUpperInvariant();
        if (normalized.Length != 3)
            throw new DomainException("Currency must be a 3-letter ISO code.");

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
