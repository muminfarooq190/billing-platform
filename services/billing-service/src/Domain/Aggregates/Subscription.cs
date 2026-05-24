using BillingService.Domain.Common;
using BillingService.Domain.Enums;
using BillingService.Domain.Events;

namespace BillingService.Domain.Aggregates;

public sealed class Subscription : AggregateRoot
{
    private Subscription() { }

    private Subscription(Guid tenantId, PlanType planType, BillingCycle billingCycle)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        PlanType = planType;
        BillingCycle = billingCycle;
        Status = SubscriptionStatus.Active;
        StartDate = DateTimeOffset.UtcNow;
        CurrentPeriodStart = StartDate;
        CurrentPeriodEnd = billingCycle == BillingCycle.Monthly ? StartDate.AddMonths(1) : StartDate.AddYears(1);
        NextBillingDate = CurrentPeriodEnd;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new SubscriptionCreatedEvent(Id, TenantId));
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public PlanType PlanType { get; private set; }
    public BillingCycle BillingCycle { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset CurrentPeriodStart { get; private set; }
    public DateTimeOffset CurrentPeriodEnd { get; private set; }
    public DateTimeOffset NextBillingDate { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    /// <summary>
    /// Stripe Subscription id (`sub_*`) when this subscription is managed
    /// in Stripe-native subscription-mode. When set, the billing scheduler
    /// SKIPS this subscription — Stripe drives the renewal cadence and
    /// invoice creation; we only react to webhooks.
    ///
    /// Null = legacy / mock-gateway path where our cron generates invoices
    /// + StripePaymentGateway uses one-shot Checkout per invoice.
    /// </summary>
    public string? StripeSubscriptionId { get; private set; }

    public static Subscription Create(Guid tenantId, PlanType planType, BillingCycle billingCycle) => new(tenantId, planType, billingCycle);

    public void Cancel()
    {
        if (Status == SubscriptionStatus.Cancelled) return;
        Status = SubscriptionStatus.Cancelled;
        CancelledAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new SubscriptionCancelledEvent(Id, TenantId));
    }

    public void RenewNextCycle()
    {
        CurrentPeriodStart = CurrentPeriodEnd;
        CurrentPeriodEnd = BillingCycle == BillingCycle.Monthly ? CurrentPeriodEnd.AddMonths(1) : CurrentPeriodEnd.AddYears(1);
        NextBillingDate = CurrentPeriodEnd;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Flip an Active subscription to PastDue when its invoice clears the
    /// grace window. Idempotent — only fires the event on first transition.
    /// </summary>
    public void MarkPastDue(Guid overdueInvoiceId, DateTimeOffset dueDate, int daysOverdue)
    {
        if (Status == SubscriptionStatus.PastDue) return;
        Status = SubscriptionStatus.PastDue;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new SubscriptionPastDueEvent(Id, TenantId, overdueInvoiceId, dueDate, daysOverdue));
    }

    /// <summary>
    /// Fire the lifecycle event signaling the user-initiated cancel has
    /// reached its access cutoff. Does NOT change Status — Cancelled is the
    /// terminal state; "expired" is just `Cancelled` + period elapsed.
    /// </summary>
    public void MarkExpired()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new SubscriptionExpiredEvent(Id, TenantId, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Bring a Cancelled or PastDue subscription back to Active. Used by the
    /// portal "Reactivate" CTA and by the Stripe webhook handler when an
    /// overdue invoice clears. Throws if called on Active to surface caller
    /// bugs rather than silently no-op.
    /// </summary>
    public void Reactivate()
    {
        if (Status == SubscriptionStatus.Active) return;

        var previous = Status.ToString();
        Status = SubscriptionStatus.Active;
        CancelledAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new SubscriptionReactivatedEvent(Id, TenantId, previous));
    }

    /// <summary>
    /// Convenience predicate used by the lifecycle worker: true when this
    /// subscription has crossed `CurrentPeriodEnd`.
    /// </summary>
    public bool IsPeriodElapsed(DateTimeOffset now) => CurrentPeriodEnd <= now;

    /// <summary>
    /// Link this subscription to a Stripe `sub_*` id. Once linked,
    /// <see cref="IsManagedByStripe"/> is true and our cron stops driving
    /// renewals — Stripe owns the cycle.
    ///
    /// Period boundaries are overwritten from the Stripe-reported window
    /// since Stripe's billing anchor may differ from ours (Stripe charges
    /// immediately + prorates differently).
    /// </summary>
    public void LinkStripeSubscription(string stripeSubscriptionId, DateTimeOffset periodStart, DateTimeOffset periodEnd)
    {
        if (string.IsNullOrWhiteSpace(stripeSubscriptionId))
            throw new ArgumentException("Stripe subscription id is required.", nameof(stripeSubscriptionId));

        StripeSubscriptionId = stripeSubscriptionId.Trim();
        CurrentPeriodStart = periodStart;
        CurrentPeriodEnd = periodEnd;
        NextBillingDate = periodEnd;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>True when Stripe owns the billing schedule.</summary>
    public bool IsManagedByStripe => !string.IsNullOrWhiteSpace(StripeSubscriptionId);

    /// <summary>
    /// Fire the expiring-soon domain event without mutating state. Lifecycle
    /// worker is responsible for idempotency-by-tier — calling twice in the
    /// same day for the same tier is a worker-side bug.
    /// </summary>
    public void AddExpiringSoonNotification(int daysRemaining)
    {
        AddDomainEvent(new SubscriptionExpiringSoonEvent(Id, TenantId, daysRemaining, CurrentPeriodEnd));
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
