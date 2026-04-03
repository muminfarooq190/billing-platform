namespace BillingService.Api.Contracts;

public sealed record CreateSubscriptionRequest(Guid TenantId, string PlanType, string BillingCycle);
