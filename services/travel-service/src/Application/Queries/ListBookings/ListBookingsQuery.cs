using MediatR;
using TravelService.Application.Queries.GetBookingById;

namespace TravelService.Application.Queries.ListBookings;

public sealed record ListBookingsQuery(
    Guid TenantId,
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    string? Destination = null,
    DateTimeOffset? StartDateFrom = null,
    DateTimeOffset? StartDateTo = null,
    Guid? AssignedToUserId = null,
    Guid? PrimaryContactId = null) : IRequest<IReadOnlyList<BookingReadModel>>;
