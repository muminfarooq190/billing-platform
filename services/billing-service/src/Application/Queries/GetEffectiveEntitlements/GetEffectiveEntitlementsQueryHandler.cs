using BillingService.Application.Abstractions;
using BillingService.Application.ReadModels;
using BillingService.Domain.Repositories;
using MediatR;

namespace BillingService.Application.Queries.GetEffectiveEntitlements;

public sealed class GetEffectiveEntitlementsQueryHandler(ISubscriptionRepository subscriptionRepository, IFeatureEntitlementRepository featureEntitlementRepository, IEntitlementResolver entitlementResolver) : IRequestHandler<GetEffectiveEntitlementsQuery, IReadOnlyList<FeatureEntitlementReadModel>>
{
    public async Task<IReadOnlyList<FeatureEntitlementReadModel>> Handle(GetEffectiveEntitlementsQuery request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByTenantIdAsync(request.TenantId, cancellationToken)
            ?? throw new InvalidOperationException("Subscription not found.");

        var overrides = await featureEntitlementRepository.ListByTenantIdAsync(request.TenantId, cancellationToken);
        var resolved = entitlementResolver.ResolveForPlan(request.TenantId, subscription.PlanType)
            .ToDictionary(x => x.FeatureKey, StringComparer.OrdinalIgnoreCase);

        var now = DateTimeOffset.UtcNow;
        foreach (var entry in overrides.Where(x => x.IsEffectiveAt(now)).OrderBy(x => x.EffectiveFrom))
        {
            resolved[entry.FeatureKey] = new FeatureEntitlementReadModel
            {
                FeatureKey = entry.FeatureKey,
                Granted = entry.Granted,
                Source = entry.Source.ToString(),
                PlanType = entry.PlanType?.ToString(),
                LimitValue = entry.LimitValue,
                EffectiveFrom = entry.EffectiveFrom,
                EffectiveTo = entry.EffectiveTo,
                MetadataJson = entry.MetadataJson
            };
        }

        return resolved.Values.OrderBy(x => x.FeatureKey).ToList();
    }
}
