using BillingService.Domain.Common;

namespace BillingService.Domain.Events;

/// <summary>
/// Fired when a Cancelled or PastDue subscription is brought back to Active
/// before its period elapses (e.g. user clicked "Reactivate" on the billing
/// page, or the failed invoice was paid).
/// </summary>
public sealed record SubscriptionReactivatedEvent(
    Guid SubscriptionId,
    Guid TenantId,
    string PreviousStatus) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
