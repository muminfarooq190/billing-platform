using BillingService.Domain.Common;

namespace BillingService.Domain.Events;

/// <summary>
/// Fired exactly once when a Cancelled subscription's period elapses (i.e.
/// the user-initiated cancel reaches its access cutoff). The portal then
/// blocks write actions via RouteGuard requireActiveSubscription.
///
/// Distinct from <see cref="SubscriptionPastDueEvent"/>, which fires for
/// failed-payment grace expiry on an otherwise Active subscription.
/// </summary>
public sealed record SubscriptionExpiredEvent(
    Guid SubscriptionId,
    Guid TenantId,
    DateTimeOffset ExpiredAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
