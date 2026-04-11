using BillingService.Application.Abstractions;
using BillingService.Application.ReadModels;
using BillingService.Domain.Enums;
using BillingService.Domain.Repositories;
using BillingService.Infrastructure.Entitlements;
using MediatR;

namespace BillingService.Application.Queries.GetEffectiveEntitlements;

public sealed class GetEffectiveEntitlementsQueryHandler(
    ISubscriptionRepository subscriptionRepository,
    IFeatureEntitlementRepository featureEntitlementRepository,
    ICommercialPackageRepository commercialPackageRepository,
    ITenantSubscriptionPackageRepository tenantSubscriptionPackageRepository,
    ITenantFeatureOverrideRepository tenantFeatureOverrideRepository,
    IEntitlementResolver entitlementResolver) : IRequestHandler<GetEffectiveEntitlementsQuery, IReadOnlyList<FeatureEntitlementReadModel>>
{
    public async Task<IReadOnlyList<FeatureEntitlementReadModel>> Handle(GetEffectiveEntitlementsQuery request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByTenantIdAsync(request.TenantId, cancellationToken)
            ?? throw new InvalidOperationException("Subscription not found.");

        var overrides = await featureEntitlementRepository.ListByTenantIdAsync(request.TenantId, cancellationToken);
        var tenantOverrides = await tenantFeatureOverrideRepository.ListByTenantIdAsync(request.TenantId, cancellationToken);
        var packageAssignments = await tenantSubscriptionPackageRepository.ListByTenantIdAsync(request.TenantId, cancellationToken);
        IReadOnlyList<FeatureEntitlementReadModel> baseResolved;

        if (packageAssignments.Count > 0)
        {
            var packages = await commercialPackageRepository.ListActiveAsync(cancellationToken);
            var packageIds = packageAssignments.Select(x => x.CommercialPackageId).Distinct().ToList();
            var packageFeatures = await commercialPackageRepository.ListFeaturesByPackageIdsAsync(packageIds, cancellationToken);
            baseResolved = FlexibleEntitlementResolver.Resolve(request.TenantId, packageAssignments, packages, packageFeatures);
        }
        else
        {
            baseResolved = entitlementResolver.ResolveForPlan(request.TenantId, subscription.PlanType);
        }

        var resolved = baseResolved.ToDictionary(x => x.FeatureKey, StringComparer.OrdinalIgnoreCase);

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

        foreach (var entry in tenantOverrides.Where(x => x.IsEffectiveAt(now)).OrderBy(x => x.EffectiveFrom))
        {
            resolved[entry.FeatureKey] = new FeatureEntitlementReadModel
            {
                FeatureKey = entry.FeatureKey,
                Granted = entry.Granted,
                Source = EntitlementSource.Override.ToString(),
                PlanType = null,
                LimitValue = entry.LimitValue,
                EffectiveFrom = entry.EffectiveFrom,
                EffectiveTo = entry.EffectiveTo,
                MetadataJson = entry.MetadataJson
            };
        }

        return resolved.Values.OrderBy(x => x.FeatureKey).ToList();
    }
}
