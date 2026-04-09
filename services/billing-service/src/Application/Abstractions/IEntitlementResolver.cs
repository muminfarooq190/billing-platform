using BillingService.Application.ReadModels;
using BillingService.Domain.Enums;

namespace BillingService.Application.Abstractions;

public interface IEntitlementResolver
{
    IReadOnlyList<FeatureEntitlementReadModel> ResolveForPlan(Guid tenantId, PlanType planType);
}
