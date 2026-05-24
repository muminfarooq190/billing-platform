using BillingService.Domain.Common;

namespace BillingService.Domain.Events;

/// <summary>
/// Fired when an Active subscription has an invoice that's been Overdue
/// past the configured grace window (default 7 days). State flips to
/// `PastDue`; portal goes read-only via the subscription gate.
/// </summary>
public sealed record SubscriptionPastDueEvent(
    Guid SubscriptionId,
    Guid TenantId,
    Guid OverdueInvoiceId,
    DateTimeOffset DueDate,
    int DaysOverdue) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
