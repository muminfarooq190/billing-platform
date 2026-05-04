using MediatR;

namespace TravelService.Application.Queries.GetBookingById;

public sealed record GetBookingByIdQuery(Guid TenantId, Guid BookingId) : IRequest<BookingReadModel?>;

public sealed class BookingReadModel
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid? QuotationId { get; init; }
    public Guid? AcceptedRevisionId { get; init; }
    public Guid PrimaryContactId { get; init; }
    public string BookingNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string TripName { get; init; } = string.Empty;
    public string Destination { get; init; } = string.Empty;
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
    public int TravellersCount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public decimal TotalSellAmount { get; init; }
    public decimal? TotalPaidAmount { get; init; }
    public decimal? TotalOutstandingAmount { get; init; }
    public decimal? TotalCostAmount { get; init; }
    public decimal? MarginAmount { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public string? CustomerReference { get; init; }
    public string? InternalNotes { get; init; }
    public string? CustomerName { get; init; }
    public int DocumentsRequired { get; init; }
    public int DocumentsUploaded { get; init; }
    public int TravelersRequired { get; init; }
    public int TravelersComplete { get; init; }
    public Guid? ItineraryId { get; init; }
    public bool HasItinerary { get; init; }
    public string? ItineraryStatus { get; init; }
    public DateTimeOffset? ItineraryUpdatedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public DateTimeOffset? CancelledAt { get; init; }
}
