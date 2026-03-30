using MediatR;

namespace IdentityService.Application.Commands.CreateUser;

public sealed record CreateUserCommand(Guid TenantId, string Email, string Password, string Role) : IRequest<Guid>;
