using BillingService.Domain.Aggregates;

namespace BillingService.Domain.Repositories;

public interface ISubscriptionRepository
{
    Task AddAsync(Subscription subscription, CancellationToken cancellationToken);
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Subscription?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Subscription>> ListDueSubscriptionsAsync(DateOnly billingDate, CancellationToken cancellationToken);
    Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken);
}
