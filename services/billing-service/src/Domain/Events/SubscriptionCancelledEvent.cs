using BillingService.Domain.Common;

namespace BillingService.Domain.Events;

public sealed record SubscriptionCancelledEvent(Guid SubscriptionId, Guid TenantId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
