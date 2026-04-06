using MediatR;

namespace IdentityService.Application.Commands.UpdateUser;

public sealed record UpdateUserCommand(Guid UserId, string Role, string? Password) : IRequest;
