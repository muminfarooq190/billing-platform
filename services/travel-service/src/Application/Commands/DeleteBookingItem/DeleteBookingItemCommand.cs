using MediatR;

namespace TravelService.Application.Commands.DeleteBookingItem;

public sealed record DeleteBookingItemCommand(Guid TenantId, Guid BookingId, Guid ItemId) : IRequest;
