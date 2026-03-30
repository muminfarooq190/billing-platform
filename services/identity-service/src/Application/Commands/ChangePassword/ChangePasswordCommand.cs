using MediatR;

namespace IdentityService.Application.Commands.ChangePassword;

public sealed record ChangePasswordCommand(Guid UserId, string NewPassword) : IRequest;
