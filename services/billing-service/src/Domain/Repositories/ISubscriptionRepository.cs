using BillingService.Domain.Aggregates;
using BillingService.Domain.Enums;

namespace BillingService.Domain.Repositories;

public interface ISubscriptionRepository
{
    Task AddAsync(Subscription subscription, CancellationToken cancellationToken);
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Subscription?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Subscription>> ListDueSubscriptionsAsync(DateOnly billingDate, CancellationToken cancellationToken);
    Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken);

    /// <summary>
    /// Subscriptions in the given statuses whose <c>CurrentPeriodEnd</c>
    /// falls inside <paramref name="windowStart"/>..<paramref name="windowEnd"/>
    /// (UTC inclusive). Used by the lifecycle worker to find candidates for
    /// expiring-soon, expired, and past-due transitions.
    /// </summary>
    Task<IReadOnlyList<Subscription>> ListWithPeriodEndingBetweenAsync(
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd,
        IReadOnlyCollection<SubscriptionStatus> statuses,
        CancellationToken cancellationToken);
}
