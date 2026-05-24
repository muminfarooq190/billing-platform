using BillingService.Domain.Aggregates;

namespace BillingService.Domain.Repositories;

public interface ITenantStripeLinkRepository
{
    Task<TenantStripeLink?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken);
    Task AddAsync(TenantStripeLink link, CancellationToken cancellationToken);
    Task UpdateAsync(TenantStripeLink link, CancellationToken cancellationToken);
}
