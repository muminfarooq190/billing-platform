using MediatR;

namespace TravelService.Application.Queries.GetBookingById;

public sealed record GetBookingByIdQuery(Guid TenantId, Guid BookingId) : IRequest<BookingReadModel?>;

public sealed record BookingReadModel(
    Guid Id,
    Guid TenantId,
    Guid? QuotationId,
    Guid? AcceptedRevisionId,
    Guid PrimaryContactId,
    string BookingNumber,
    string Status,
    string TripName,
    string Destination,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    int TravellersCount,
    string Currency,
    decimal TotalSellAmount,
    decimal? TotalCostAmount,
    decimal? MarginAmount,
    Guid? AssignedToUserId,
    string? CustomerReference,
    string? InternalNotes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? CancelledAt);
