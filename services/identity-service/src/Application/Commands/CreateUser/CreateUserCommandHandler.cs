using IdentityService.Application.Abstractions;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.Enums;
using IdentityService.Domain.Exceptions;
using IdentityService.Domain.Repositories;
using IdentityService.Domain.ValueObjects;
using MediatR;

namespace IdentityService.Application.Commands.CreateUser;

public sealed class CreateUserCommandHandler(
    ITenantRepository tenantRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateUserCommand, Guid>
{
    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException("Tenant not found.");

        var existingUser = await userRepository.GetByTenantAndEmailAsync(request.TenantId, request.Email, cancellationToken);
        if (existingUser is not null)
        {
            throw new ConflictException("User already exists for this tenant.");
        }

        var role = Enum.Parse<UserRole>(request.Role, true);
        var user = User.Create(new TenantId(tenant.Id), new Email(request.Email), BCrypt.Net.BCrypt.HashPassword(request.Password), role);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}
