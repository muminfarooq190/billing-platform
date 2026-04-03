using IdentityService.Application.Abstractions;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.Enums;
using IdentityService.Domain.Exceptions;
using IdentityService.Domain.Repositories;
using IdentityService.Domain.ValueObjects;
using MediatR;

namespace IdentityService.Application.Commands.RegisterTenant;

public sealed class RegisterTenantCommandHandler(
    ITenantRepository tenantRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterTenantCommand, RegisterTenantResult>
{
    public async Task<RegisterTenantResult> Handle(RegisterTenantCommand request, CancellationToken cancellationToken)
    {
        var existing = await tenantRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException("A tenant with this email already exists.");
        }

        var tenant = Tenant.Register(request.TenantName, new Email(request.Email), TenantPlan.Free);
        var owner = User.Create(new TenantId(tenant.Id), new Email(request.Email), BCrypt.Net.BCrypt.HashPassword(request.Password), UserRole.Owner);

        await tenantRepository.AddAsync(tenant, cancellationToken);
        await userRepository.AddAsync(owner, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterTenantResult(tenant.Id, owner.Id);
    }
}
