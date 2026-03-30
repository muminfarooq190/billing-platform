using BillingService.Domain.Common;

namespace BillingService.Domain.Events;

public sealed record InvoicePaidEvent(Guid InvoiceId, Guid TenantId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
