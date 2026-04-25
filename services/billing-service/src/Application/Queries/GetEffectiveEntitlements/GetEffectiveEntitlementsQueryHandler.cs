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
    ITenantFeatureOverrideRepository tenantFeatureOverrideRepository) : IRequestHandler<GetEffectiveEntitlementsQuery, IReadOnlyList<FeatureEntitlementReadModel>>
{
    public async Task<IReadOnlyList<FeatureEntitlementReadModel>> Handle(GetEffectiveEntitlementsQuery request, CancellationToken cancellationToken)
    {
        _ = await subscriptionRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);

        var overrides = await featureEntitlementRepository.ListByTenantIdAsync(request.TenantId, cancellationToken);
        var tenantOverrides = await tenantFeatureOverrideRepository.ListByTenantIdAsync(request.TenantId, cancellationToken);
        var packageAssignments = await tenantSubscriptionPackageRepository.ListByTenantIdAsync(request.TenantId, cancellationToken);
        var packages = await commercialPackageRepository.ListActiveAsync(cancellationToken);
        var packageIds = packageAssignments.Select(x => x.CommercialPackageId).Distinct().ToList();
        var packageFeatures = await commercialPackageRepository.ListFeaturesByPackageIdsAsync(packageIds, cancellationToken);
        var baseResolved = FlexibleEntitlementResolver.Resolve(request.TenantId, packageAssignments, packages, packageFeatures);

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
