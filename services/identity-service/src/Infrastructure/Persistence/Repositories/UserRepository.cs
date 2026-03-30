using IdentityService.Domain.Aggregates;
using IdentityService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(IdentityDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await dbContext.Users.SingleOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

    public async Task<User?> GetByTenantAndEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken)
        => await dbContext.Users.SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Email == email && x.DeletedAt == null, cancellationToken);

    public async Task<IReadOnlyList<User>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken)
        => await dbContext.Users.Where(x => x.TenantId == tenantId && x.DeletedAt == null).ToListAsync(cancellationToken);

    public Task AddAsync(User user, CancellationToken cancellationToken)
        => dbContext.Users.AddAsync(user, cancellationToken).AsTask();

    public Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        dbContext.Users.Update(user);
        return Task.CompletedTask;
    }
}
