using IdentityService.Domain.Aggregates;

namespace IdentityService.Domain.Repositories;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Tenant?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken);
    Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken);
}
