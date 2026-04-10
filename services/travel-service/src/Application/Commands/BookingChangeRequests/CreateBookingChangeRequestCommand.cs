using MediatR;

namespace TravelService.Application.Commands.BookingChangeRequests;

public sealed record CreateBookingChangeRequestCommand(Guid TenantId, Guid BookingId, string ChangeType, string Reason) : IRequest<Guid>;
