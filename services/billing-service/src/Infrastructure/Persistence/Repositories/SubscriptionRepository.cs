using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Persistence.Repositories;

public sealed class SubscriptionRepository(BillingDbContext dbContext) : ISubscriptionRepository
{
    public Task AddAsync(Subscription subscription, CancellationToken cancellationToken) => dbContext.Subscriptions.AddAsync(subscription, cancellationToken).AsTask();

    public Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => dbContext.Subscriptions.SingleOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

    public Task<Subscription?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => dbContext.Subscriptions.SingleOrDefaultAsync(x => x.TenantId == tenantId && x.DeletedAt == null, cancellationToken);

    public async Task<IReadOnlyList<Subscription>> ListDueSubscriptionsAsync(DateOnly billingDate, CancellationToken cancellationToken)
    {
        var utc = billingDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        return await dbContext.Subscriptions.Where(x => x.NextBillingDate <= utc && x.Status == Domain.Enums.SubscriptionStatus.Active && x.DeletedAt == null).ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        dbContext.Subscriptions.Update(subscription);
        return Task.CompletedTask;
    }
}
