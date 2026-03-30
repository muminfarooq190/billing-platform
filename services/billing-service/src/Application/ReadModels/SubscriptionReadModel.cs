namespace BillingService.Application.ReadModels;

public sealed record SubscriptionReadModel(Guid Id, Guid TenantId, string PlanType, string BillingCycle, string Status, DateTimeOffset NextBillingDate);
