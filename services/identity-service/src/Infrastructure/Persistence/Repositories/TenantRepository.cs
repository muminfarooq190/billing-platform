using IdentityService.Domain.Aggregates;
using IdentityService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository(IdentityDbContext dbContext) : ITenantRepository
{
    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await dbContext.Tenants.SingleOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

    public async Task<Tenant?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        => await dbContext.Tenants.SingleOrDefaultAsync(x => x.Email == email && x.DeletedAt == null, cancellationToken);

    public Task AddAsync(Tenant tenant, CancellationToken cancellationToken)
        => dbContext.Tenants.AddAsync(tenant, cancellationToken).AsTask();

    public Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        dbContext.Tenants.Update(tenant);
        return Task.CompletedTask;
    }
}
