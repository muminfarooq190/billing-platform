namespace BillingService.Application.ReadModels;

public sealed class SubscriptionReadModel
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string PlanType { get; init; } = string.Empty;
    public string BillingCycle { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset NextBillingDate { get; init; }
}
