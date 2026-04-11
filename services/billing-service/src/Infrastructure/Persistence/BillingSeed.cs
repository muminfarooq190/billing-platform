using BillingService.Domain.Aggregates;
using BillingService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Persistence;

public static class BillingSeed
{
    public static async Task SeedFlexibleEntitlementsAsync(BillingDbContext dbContext, CancellationToken cancellationToken)
    {
        await SeedFeatureCatalogAsync(dbContext, cancellationToken);
        await SeedPlanPackagesAsync(dbContext, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedFeatureCatalogAsync(BillingDbContext dbContext, CancellationToken cancellationToken)
    {
        var existingKeys = await dbContext.FeatureCatalog
            .AsNoTracking()
            .Select(x => x.FeatureKey)
            .ToListAsync(cancellationToken);

        var existing = existingKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var entries = new[]
        {
            FeatureCatalogEntry.Create("travel.quotation.create", "travel-service", "travel", "Create quotation", "Allows tenant to create quotations."),
            FeatureCatalogEntry.Create("travel.quotation.send", "travel-service", "travel", "Send quotation", "Allows tenant to send quotations."),
            FeatureCatalogEntry.Create("travel.booking.create", "travel-service", "travel", "Create booking", "Allows tenant to create bookings."),
            FeatureCatalogEntry.Create("travel.timeline.read", "travel-service", "travel", "Read timeline", "Allows tenant to read travel timeline and work queue."),
            FeatureCatalogEntry.Create("travel.notes.write", "travel-service", "travel", "Write notes", "Allows tenant to create and update travel notes."),
            FeatureCatalogEntry.Create("travel.audit.read", "travel-service", "travel", "Read audit/reporting", "Allows tenant to access travel audit/reporting."),
            FeatureCatalogEntry.Create("communication.notification.send", "communication-service", "communication", "Send notifications", "Allows tenant to send notifications.", true, "messages"),
            FeatureCatalogEntry.Create("branding.theme.manage", "identity-service", "branding", "Manage theme", "Allows tenant to manage branding theme."),
            FeatureCatalogEntry.Create("branding.assets.manage", "identity-service", "branding", "Manage brand assets", "Allows tenant to manage branding assets.", true, "assets")
        };

        var missingEntries = entries.Where(x => !existing.Contains(x.FeatureKey)).ToList();
        if (missingEntries.Count > 0)
        {
            dbContext.FeatureCatalog.AddRange(missingEntries);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task SeedPlanPackagesAsync(BillingDbContext dbContext, CancellationToken cancellationToken)
    {
        var packageDefinitions = new[]
        {
            new { Code = "legacy.free", Name = "Legacy Free", Plan = PlanType.Free, Description = "Legacy compatibility package for Free plan." },
            new { Code = "legacy.pro", Name = "Legacy Pro", Plan = PlanType.Pro, Description = "Legacy compatibility package for Pro plan." },
            new { Code = "legacy.enterprise", Name = "Legacy Enterprise", Plan = PlanType.Enterprise, Description = "Legacy compatibility package for Enterprise plan." }
        };

        var existingPackages = await dbContext.CommercialPackages
            .ToDictionaryAsync(x => x.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var definition in packageDefinitions)
        {
            if (!existingPackages.TryGetValue(definition.Code, out var package))
            {
                package = CommercialPackage.Create(definition.Code, definition.Name, "BasePlan", "Flat", definition.Description);
                dbContext.CommercialPackages.Add(package);
                await dbContext.SaveChangesAsync(cancellationToken);
                existingPackages[definition.Code] = package;
            }

            var hasFeatures = await dbContext.CommercialPackageFeatures
                .AnyAsync(x => x.CommercialPackageId == package.Id, cancellationToken);

            if (!hasFeatures)
            {
                dbContext.CommercialPackageFeatures.AddRange(CreateLegacyFeatures(package.Id, definition.Plan));
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
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
