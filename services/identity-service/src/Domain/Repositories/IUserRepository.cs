using IdentityService.Domain.Aggregates;

namespace IdentityService.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetByTenantAndEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken);
    Task<IReadOnlyList<User>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task UpdateAsync(User user, CancellationToken cancellationToken);
}
