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
        NextBillingDate = billingCycle == BillingCycle.Monthly ? StartDate.AddMonths(1) : StartDate.AddYears(1);
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
    public DateTimeOffset NextBillingDate { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

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
        NextBillingDate = BillingCycle == BillingCycle.Monthly ? NextBillingDate.AddMonths(1) : NextBillingDate.AddYears(1);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
