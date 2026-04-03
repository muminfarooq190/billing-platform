using MediatR;

namespace IdentityService.Application.Commands.RegisterTenant;

public sealed record RegisterTenantCommand(string TenantName, string Email, string Password) : IRequest<RegisterTenantResult>;

public sealed record RegisterTenantResult(Guid TenantId, Guid OwnerUserId);
