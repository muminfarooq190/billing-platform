using BillingService.Application.Abstractions;
using BillingService.Application.ReadModels;
using BillingService.Domain.Enums;

namespace BillingService.Infrastructure.Entitlements;

public sealed class PlanEntitlementResolver : IEntitlementResolver
{
    public IReadOnlyList<FeatureEntitlementReadModel> ResolveForPlan(Guid tenantId, PlanType planType)
    {
        var now = DateTimeOffset.UtcNow;
        return GetDefinitions(planType)
            .Select(x => new FeatureEntitlementReadModel
            {
                FeatureKey = x.FeatureKey,
                Granted = x.Granted,
                Source = EntitlementSource.Plan.ToString(),
                PlanType = planType.ToString(),
                LimitValue = x.LimitValue,
                EffectiveFrom = now,
                EffectiveTo = null,
                MetadataJson = null
            })
            .ToList();
    }

    private static IReadOnlyList<(string FeatureKey, bool Granted, int? LimitValue)> GetDefinitions(PlanType planType)
        => planType switch
        {
            PlanType.Free =>
            [
                ("travel.quotation.create", false, null),
                ("travel.quotation.send", false, null),
                ("travel.booking.create", false, null),
                ("travel.timeline.read", false, null),
                ("travel.notes.write", false, null),
                ("travel.audit.read", false, null),
                ("communication.notification.send", false, null),
                ("branding.theme.manage", false, null),
                ("branding.assets.manage", false, null)
            ],
            PlanType.Pro =>
            [
                ("travel.quotation.create", true, null),
                ("travel.quotation.send", true, null),
                ("travel.booking.create", true, null),
                ("travel.timeline.read", true, null),
                ("travel.notes.write", true, null),
                ("travel.audit.read", false, null),
                ("communication.notification.send", true, 5000),
                ("branding.theme.manage", true, null),
                ("branding.assets.manage", true, 25)
            ],
            PlanType.Enterprise =>
            [
                ("travel.quotation.create", true, null),
                ("travel.quotation.send", true, null),
                ("travel.booking.create", true, null),
                ("travel.timeline.read", true, null),
                ("travel.notes.write", true, null),
                ("travel.audit.read", true, null),
                ("communication.notification.send", true, null),
                ("branding.theme.manage", true, null),
                ("branding.assets.manage", true, null)
            ],
            _ => []
        };
}
