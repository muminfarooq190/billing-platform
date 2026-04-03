using MediatR;

namespace IdentityService.Application.Commands.SuspendTenant;

public sealed record SuspendTenantCommand(Guid TenantId) : IRequest;
