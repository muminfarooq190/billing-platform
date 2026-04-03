using IdentityService.Application.Abstractions;
using IdentityService.Domain.Exceptions;
using IdentityService.Domain.Repositories;
using MediatR;

namespace IdentityService.Application.Commands.SuspendTenant;

public sealed class SuspendTenantCommandHandler(
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SuspendTenantCommand>
{
    public async Task Handle(SuspendTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException("Tenant not found.");

        tenant.Suspend();
        await tenantRepository.UpdateAsync(tenant, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
