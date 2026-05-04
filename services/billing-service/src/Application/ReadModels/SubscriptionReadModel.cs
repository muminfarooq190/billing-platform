namespace BillingService.Application.ReadModels;

public sealed class SubscriptionReadModel
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string PlanType { get; init; } = string.Empty;
    public string BillingCycle { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset StartsAt { get; init; }
    public DateTimeOffset? EndsAt { get; init; }
    public DateTimeOffset CurrentPeriodStart { get; init; }
    public DateTimeOffset CurrentPeriodEnd { get; init; }
    public DateTimeOffset NextBillingDate { get; init; }
}
