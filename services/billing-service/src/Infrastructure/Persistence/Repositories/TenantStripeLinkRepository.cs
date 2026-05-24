using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Persistence.Repositories;

public sealed class TenantStripeLinkRepository(BillingDbContext dbContext) : ITenantStripeLinkRepository
{
    public Task<TenantStripeLink?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken)
        => dbContext.TenantStripeLinks.SingleOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

    public Task AddAsync(TenantStripeLink link, CancellationToken cancellationToken)
        => dbContext.TenantStripeLinks.AddAsync(link, cancellationToken).AsTask();

    public Task UpdateAsync(TenantStripeLink link, CancellationToken cancellationToken)
    {
        dbContext.TenantStripeLinks.Update(link);
        return Task.CompletedTask;
    }
}
