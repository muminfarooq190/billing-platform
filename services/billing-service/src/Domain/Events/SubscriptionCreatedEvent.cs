using BillingService.Domain.Common;

namespace BillingService.Domain.Events;

public sealed record SubscriptionCreatedEvent(Guid SubscriptionId, Guid TenantId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
