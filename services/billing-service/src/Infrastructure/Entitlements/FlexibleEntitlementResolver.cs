using BillingService.Application.ReadModels;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Enums;

namespace BillingService.Infrastructure.Entitlements;

public static class FlexibleEntitlementResolver
{
    public static IReadOnlyList<FeatureEntitlementReadModel> Resolve(Guid tenantId, IReadOnlyList<TenantSubscriptionPackage> assignments, IReadOnlyList<CommercialPackage> packages, IReadOnlyList<CommercialPackageFeature> packageFeatures)
    {
        var now = DateTimeOffset.UtcNow;
        var activeAssignments = assignments.Where(x => x.IsEffectiveAt(now)).ToList();
        if (activeAssignments.Count == 0)
        {
            return [];
        }

        var packageIds = activeAssignments.Select(x => x.CommercialPackageId).ToHashSet();
        var activePackages = packages.Where(x => packageIds.Contains(x.Id) && x.IsActive && x.DeletedAt == null).ToList();
        var activePackageIds = activePackages.Select(x => x.Id).ToHashSet();

        var grouped = packageFeatures
            .Where(x => activePackageIds.Contains(x.CommercialPackageId))
            .GroupBy(x => x.FeatureKey, StringComparer.OrdinalIgnoreCase);

        return grouped.Select(group =>
        {
            var entries = group.ToList();
            return new FeatureEntitlementReadModel
            {
                FeatureKey = group.Key,
                Granted = entries.Any(x => x.Granted),
                Source = EntitlementSource.Plan.ToString(),
                PlanType = null,
                LimitValue = ResolveLimit(entries),
                EffectiveFrom = now,
                EffectiveTo = null,
                MetadataJson = null
            };
        })
        .OrderBy(x => x.FeatureKey)
        .ToList();
    }

    private static int? ResolveLimit(IReadOnlyList<CommercialPackageFeature> entries)
    {
        var limitedEntries = entries.Where(x => x.LimitValue.HasValue).ToList();
        if (limitedEntries.Count == 0)
        {
            return null;
        }

        var policy = limitedEntries.Last().LimitMergePolicy;
        return policy switch
        {
            LimitMergePolicy.Sum => limitedEntries.Sum(x => x.LimitValue ?? 0),
            LimitMergePolicy.LatestWins => limitedEntries.Last().LimitValue,
            LimitMergePolicy.OverrideOnly => limitedEntries.Last().LimitValue,
            _ => limitedEntries.Max(x => x.LimitValue)
        };
    }
}
