using MediatR;

namespace IdentityService.Application.Commands.DeleteUser;

public sealed record DeleteUserCommand(Guid UserId) : IRequest;
