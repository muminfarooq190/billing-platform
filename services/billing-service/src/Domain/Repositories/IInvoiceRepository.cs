using BillingService.Domain.Aggregates;

namespace BillingService.Domain.Repositories;

public interface IInvoiceRepository
{
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken);
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Invoice?> GetBySubscriptionAndBillingPeriodAsync(Guid subscriptionId, DateOnly billingPeriodStart, DateOnly billingPeriodEnd, CancellationToken cancellationToken);
    Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken);
    Task<IReadOnlyList<Invoice>> ListOverdueCandidatesAsync(DateTimeOffset utcNow, CancellationToken cancellationToken);
}
