using MediatR;
using TravelService.Application.Queries.GetBookingById;

namespace TravelService.Application.Queries.ListBookings;

public sealed record ListBookingsQuery(Guid TenantId) : IRequest<IReadOnlyList<BookingReadModel>>;
