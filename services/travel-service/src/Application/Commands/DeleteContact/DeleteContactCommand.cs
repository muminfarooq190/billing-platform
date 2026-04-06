using MediatR;

namespace TravelService.Application.Commands.DeleteContact;

public sealed record DeleteContactCommand(Guid Id) : IRequest;
