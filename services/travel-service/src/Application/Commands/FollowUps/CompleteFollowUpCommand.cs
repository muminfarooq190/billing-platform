using MediatR;

namespace TravelService.Application.Commands.FollowUps;

public sealed record CompleteFollowUpCommand(Guid FollowUpId) : IRequest;
