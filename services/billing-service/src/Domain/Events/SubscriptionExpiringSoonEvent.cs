using BillingService.Domain.Common;

namespace BillingService.Domain.Events;

/// <summary>
/// Fired once per warning tier (7d/3d/1d) when a subscription's current
/// billing period is approaching `CurrentPeriodEnd`. Consumed by
/// communication-service to email/push the tenant owner, and surfaced on
/// the portal via the expiry banner.
///
/// `DaysRemaining` is the integer day count at the tier boundary so
/// consumers can pick the right template (e.g. "1 day left" vs "7 days
/// left"). Idempotency is by (SubscriptionId, DaysRemaining) — the worker
/// records emitted tiers and skips re-fires.
/// </summary>
public sealed record SubscriptionExpiringSoonEvent(
    Guid SubscriptionId,
    Guid TenantId,
    int DaysRemaining,
    DateTimeOffset CurrentPeriodEnd) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
