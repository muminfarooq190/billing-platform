using MediatR;

namespace TravelService.Application.Commands.FollowUps;

public sealed record ReassignFollowUpCommand(Guid FollowUpId, Guid? AssignedToUserId) : IRequest;
