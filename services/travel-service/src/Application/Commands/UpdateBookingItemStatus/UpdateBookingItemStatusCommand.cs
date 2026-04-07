using MediatR;

namespace TravelService.Application.Commands.UpdateBookingItemStatus;

public sealed record UpdateBookingItemStatusCommand(Guid TenantId, Guid BookingId, Guid ItemId, string Status) : IRequest;
