using BillingService.Domain.Aggregates;
using BillingService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Persistence;

public static class BillingSeed
{
    public static async Task SeedFlexibleEntitlementsAsync(BillingDbContext dbContext, CancellationToken cancellationToken)
    {
        if (!await dbContext.FeatureCatalog.AnyAsync(cancellationToken))
        {
            dbContext.FeatureCatalog.AddRange(
                FeatureCatalogEntry.Create("travel.quotation.create", "travel-service", "travel", "Create quotation", "Allows tenant to create quotations."),
                FeatureCatalogEntry.Create("travel.quotation.send", "travel-service", "travel", "Send quotation", "Allows tenant to send quotations."),
                FeatureCatalogEntry.Create("travel.booking.create", "travel-service", "travel", "Create booking", "Allows tenant to create bookings."),
                FeatureCatalogEntry.Create("travel.timeline.read", "travel-service", "travel", "Read timeline", "Allows tenant to read travel timeline and work queue."),
                FeatureCatalogEntry.Create("travel.notes.write", "travel-service", "travel", "Write notes", "Allows tenant to create and update travel notes."),
                FeatureCatalogEntry.Create("travel.audit.read", "travel-service", "travel", "Read audit/reporting", "Allows tenant to access travel audit/reporting."),
                FeatureCatalogEntry.Create("communication.notification.send", "communication-service", "communication", "Send notifications", "Allows tenant to send notifications.", true, "messages"),
                FeatureCatalogEntry.Create("branding.theme.manage", "identity-service", "branding", "Manage theme", "Allows tenant to manage branding theme."),
                FeatureCatalogEntry.Create("branding.assets.manage", "identity-service", "branding", "Manage brand assets", "Allows tenant to manage branding assets.", true, "assets"));
        }

        if (!await dbContext.CommercialPackages.AnyAsync(cancellationToken))
        {
            var free = CommercialPackage.Create("legacy.free", "Legacy Free", "BasePlan", "Flat", "Legacy compatibility package for Free plan.");
            var pro = CommercialPackage.Create("legacy.pro", "Legacy Pro", "BasePlan", "Flat", "Legacy compatibility package for Pro plan.");
            var enterprise = CommercialPackage.Create("legacy.enterprise", "Legacy Enterprise", "BasePlan", "Flat", "Legacy compatibility package for Enterprise plan.");
            dbContext.CommercialPackages.AddRange(free, pro, enterprise);
            await dbContext.SaveChangesAsync(cancellationToken);

            dbContext.CommercialPackageFeatures.AddRange(
                CreateLegacyFeatures(free.Id, PlanType.Free)
                    .Concat(CreateLegacyFeatures(pro.Id, PlanType.Pro))
                    .Concat(CreateLegacyFeatures(enterprise.Id, PlanType.Enterprise)));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyList<CommercialPackageFeature> CreateLegacyFeatures(Guid packageId, PlanType planType)
        => planType switch
        {
            PlanType.Free =>
            [
                CommercialPackageFeature.Create(packageId, "travel.quotation.create", false),
                CommercialPackageFeature.Create(packageId, "travel.quotation.send", false),
                CommercialPackageFeature.Create(packageId, "travel.booking.create", false),
                CommercialPackageFeature.Create(packageId, "travel.timeline.read", false),
                CommercialPackageFeature.Create(packageId, "travel.notes.write", false),
                CommercialPackageFeature.Create(packageId, "travel.audit.read", false),
                CommercialPackageFeature.Create(packageId, "communication.notification.send", false),
                CommercialPackageFeature.Create(packageId, "branding.theme.manage", false),
                CommercialPackageFeature.Create(packageId, "branding.assets.manage", false)
            ],
            PlanType.Pro =>
            [
                CommercialPackageFeature.Create(packageId, "travel.quotation.create", true),
                CommercialPackageFeature.Create(packageId, "travel.quotation.send", true),
                CommercialPackageFeature.Create(packageId, "travel.booking.create", true),
                CommercialPackageFeature.Create(packageId, "travel.timeline.read", true),
                CommercialPackageFeature.Create(packageId, "travel.notes.write", true),
                CommercialPackageFeature.Create(packageId, "travel.audit.read", false),
                CommercialPackageFeature.Create(packageId, "communication.notification.send", true, 5000),
                CommercialPackageFeature.Create(packageId, "branding.theme.manage", true),
                CommercialPackageFeature.Create(packageId, "branding.assets.manage", true, 25)
            ],
            PlanType.Enterprise =>
            [
                CommercialPackageFeature.Create(packageId, "travel.quotation.create", true),
                CommercialPackageFeature.Create(packageId, "travel.quotation.send", true),
                CommercialPackageFeature.Create(packageId, "travel.booking.create", true),
                CommercialPackageFeature.Create(packageId, "travel.timeline.read", true),
                CommercialPackageFeature.Create(packageId, "travel.notes.write", true),
                CommercialPackageFeature.Create(packageId, "travel.audit.read", true),
                CommercialPackageFeature.Create(packageId, "communication.notification.send", true),
                CommercialPackageFeature.Create(packageId, "branding.theme.manage", true),
                CommercialPackageFeature.Create(packageId, "branding.assets.manage", true)
            ],
            _ => []
        };
}
