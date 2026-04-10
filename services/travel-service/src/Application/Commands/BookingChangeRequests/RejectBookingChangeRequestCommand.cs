using MediatR;

namespace TravelService.Application.Commands.BookingChangeRequests;

public sealed record RejectBookingChangeRequestCommand(Guid TenantId, Guid BookingId, Guid ChangeRequestId, string? Reason) : IRequest;
