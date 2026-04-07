using MediatR;

namespace TravelService.Application.Commands.DeleteTraveler;

public sealed record DeleteTravelerCommand(Guid TenantId, Guid BookingId, Guid TravelerId) : IRequest;
