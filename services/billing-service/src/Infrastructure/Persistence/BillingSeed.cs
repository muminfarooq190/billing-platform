using System.Text.Json;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using static BillingService.Infrastructure.Persistence.IdentitySeedBridge;

namespace BillingService.Infrastructure.Persistence;

public static class BillingSeed
{
    public static async Task SeedFlexibleEntitlementsAsync(BillingDbContext dbContext, CancellationToken cancellationToken)
    {
        await EnsureFlexibleEntitlementSchemaAsync(dbContext, cancellationToken);
        await SeedFeatureCatalogAsync(dbContext, cancellationToken);
        await SeedPlanPackagesAsync(dbContext, cancellationToken);
        await EnsureDemoTenantEnterpriseEntitlementsAsync(dbContext, cancellationToken);
        await BackfillTenantPlanAssignmentsAsync(dbContext, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureFlexibleEntitlementSchemaAsync(BillingDbContext dbContext, CancellationToken cancellationToken)
    {
        if (!await HasColumnAsync(dbContext, "feature_catalog", "assignment_mode", cancellationToken))
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                "ALTER TABLE feature_catalog ADD COLUMN assignment_mode character varying(50) NOT NULL DEFAULT 'TenantWide';",
                cancellationToken);
        }

        if (!await HasColumnAsync(dbContext, "feature_catalog", "default_assignment_limit", cancellationToken))
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                "ALTER TABLE feature_catalog ADD COLUMN default_assignment_limit integer NULL;",
                cancellationToken);
        }

        if (!await HasColumnAsync(dbContext, "commercial_package_features", "limit_merge_policy", cancellationToken))
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                "ALTER TABLE commercial_package_features ADD COLUMN limit_merge_policy character varying(50) NOT NULL DEFAULT 'Max';",
                cancellationToken);
        }

        if (!await HasColumnAsync(dbContext, "subscriptions", "current_period_start", cancellationToken))
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                "ALTER TABLE subscriptions ADD COLUMN current_period_start timestamp with time zone NOT NULL DEFAULT NOW();",
                cancellationToken);

            await dbContext.Database.ExecuteSqlRawAsync(
                "UPDATE subscriptions SET current_period_start = start_date WHERE current_period_start IS NULL OR current_period_start = NOW();",
                cancellationToken);
        }

        if (!await HasColumnAsync(dbContext, "subscriptions", "current_period_end", cancellationToken))
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                "ALTER TABLE subscriptions ADD COLUMN current_period_end timestamp with time zone NOT NULL DEFAULT NOW();",
                cancellationToken);

            await dbContext.Database.ExecuteSqlRawAsync(
                "UPDATE subscriptions SET current_period_end = next_billing_date WHERE current_period_end IS NULL OR current_period_end = NOW();",
                cancellationToken);
        }

        if (!await HasColumnAsync(dbContext, "invoices", "invoice_number", cancellationToken))
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                "ALTER TABLE invoices ADD COLUMN invoice_number character varying(40) NOT NULL DEFAULT '';",
                cancellationToken);
        }
    }

    private static async Task<bool> HasColumnAsync(BillingDbContext dbContext, string tableName, string columnName, CancellationToken cancellationToken)
    {
        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            select 1
            from information_schema.columns
            where table_schema = 'public' and table_name = @tableName and column_name = @columnName
            limit 1;";
        command.Parameters.AddWithValue("tableName", tableName);
        command.Parameters.AddWithValue("columnName", columnName);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null;
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
            FeatureCatalogEntry.Create("travel.inquiries", "travel-service", "travel", "Read inquiries", "Allows tenant to view and manage inquiries.", assignmentMode: FeatureAssignmentMode.TenantWide),
            FeatureCatalogEntry.Create("travel.contacts", "travel-service", "travel", "Read contacts", "Allows tenant to view travel contacts.", assignmentMode: FeatureAssignmentMode.TenantWide),
            FeatureCatalogEntry.Create("travel.quotations", "travel-service", "travel", "Read quotations", "Allows tenant to view and manage quotations.", assignmentMode: FeatureAssignmentMode.TenantWide),
            FeatureCatalogEntry.Create("travel.bookings", "travel-service", "travel", "Read bookings", "Allows tenant to view and manage bookings.", assignmentMode: FeatureAssignmentMode.TenantWide),
            FeatureCatalogEntry.Create("travel.itineraries", "travel-service", "travel", "Read itineraries", "Allows tenant to view itineraries.", assignmentMode: FeatureAssignmentMode.TenantWide),
            FeatureCatalogEntry.Create("travel.followups", "travel-service", "travel", "Read follow-ups", "Allows tenant to view follow-up workflows.", assignmentMode: FeatureAssignmentMode.TenantWide),
            FeatureCatalogEntry.Create("communication.notifications", "communication-service", "communication", "Read notifications", "Allows tenant to view and manage notifications.", assignmentMode: FeatureAssignmentMode.TenantWide),
            FeatureCatalogEntry.Create("communication.logs.read", "communication-service", "communication", "Read communication logs", "Allows tenant to view notification logs and unread counts.", assignmentMode: FeatureAssignmentMode.TenantWide),
            FeatureCatalogEntry.Create("communication.templates.manage", "communication-service", "communication", "Manage communication templates", "Allows tenant to manage communication templates.", assignmentMode: FeatureAssignmentMode.TenantWide),
            FeatureCatalogEntry.Create("billing.portal", "billing-service", "billing", "Access billing portal", "Allows tenant to access billing, invoices, and package visibility.", assignmentMode: FeatureAssignmentMode.TenantWide),
            FeatureCatalogEntry.Create("identity.users.manage", "identity-service", "identity", "Manage users", "Allows tenant to manage users and feature assignments.", assignmentMode: FeatureAssignmentMode.TenantWide),
            FeatureCatalogEntry.Create("identity.settings.manage", "identity-service", "identity", "Manage settings", "Allows tenant to manage tenant settings.", assignmentMode: FeatureAssignmentMode.TenantWide),
            FeatureCatalogEntry.Create("tenant.branding", "identity-service", "branding", "Manage branding", "Allows tenant to manage branding settings.", assignmentMode: FeatureAssignmentMode.TenantWide),
            FeatureCatalogEntry.Create("integration.webhooks", "integration-service", "integration", "Manage webhooks", "Allows tenant to manage webhook integrations.", assignmentMode: FeatureAssignmentMode.ExplicitUserAssignment),
            FeatureCatalogEntry.Create("travel.quotation.create", "travel-service", "travel", "Create quotation", "Allows tenant to create quotations.", assignmentMode: FeatureAssignmentMode.ExplicitUserAssignment),
            FeatureCatalogEntry.Create("travel.quotation.send", "travel-service", "travel", "Send quotation", "Allows tenant to send quotations.", assignmentMode: FeatureAssignmentMode.ExplicitUserAssignment),
            FeatureCatalogEntry.Create("travel.booking.create", "travel-service", "travel", "Create booking", "Allows tenant to create bookings.", assignmentMode: FeatureAssignmentMode.ExplicitUserAssignment),
            FeatureCatalogEntry.Create("travel.timeline.read", "travel-service", "travel", "Read timeline", "Allows tenant to read travel timeline and work queue.", assignmentMode: FeatureAssignmentMode.TenantWide),
            FeatureCatalogEntry.Create("travel.notes.write", "travel-service", "travel", "Write notes", "Allows tenant to create and update travel notes.", assignmentMode: FeatureAssignmentMode.ExplicitUserAssignment),
            FeatureCatalogEntry.Create("travel.audit.read", "travel-service", "travel", "Read audit/reporting", "Allows tenant to access travel audit/reporting.", assignmentMode: FeatureAssignmentMode.ExplicitUserAssignment),
            FeatureCatalogEntry.Create("communication.notification.send", "communication-service", "communication", "Send notifications", "Allows tenant to send notifications.", true, "messages", assignmentMode: FeatureAssignmentMode.SeatLimitedAssignment, defaultAssignmentLimit: 2),
            FeatureCatalogEntry.Create("branding.theme.manage", "identity-service", "branding", "Manage theme", "Allows tenant to manage branding theme.", assignmentMode: FeatureAssignmentMode.ExplicitUserAssignment),
            FeatureCatalogEntry.Create("branding.assets.manage", "identity-service", "branding", "Manage brand assets", "Allows tenant to manage branding assets.", true, "assets", assignmentMode: FeatureAssignmentMode.SeatLimitedAssignment, defaultAssignmentLimit: 3)
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
                package = CommercialPackage.Create(
                    definition.Code,
                    definition.Name,
                    "BasePlan",
                    "Flat",
                    definition.Description,
                    true,
                    BuildLegacyPricingMetadata(definition.Plan));
                dbContext.CommercialPackages.Add(package);
                await dbContext.SaveChangesAsync(cancellationToken);
                existingPackages[definition.Code] = package;
            }
            else if (string.IsNullOrWhiteSpace(package.MetadataJson))
            {
                package.Update(package.Code, package.Name, package.Category, package.BillingModel, package.Description, package.IsActive, BuildLegacyPricingMetadata(definition.Plan));
                await dbContext.SaveChangesAsync(cancellationToken);
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

    private static async Task EnsureDemoTenantEnterpriseEntitlementsAsync(BillingDbContext dbContext, CancellationToken cancellationToken)
    {
        var enterprisePackage = await dbContext.CommercialPackages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == "legacy.enterprise", cancellationToken);

        if (enterprisePackage is null)
        {
            return;
        }

        var existingAssignment = await dbContext.TenantSubscriptionPackages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == DemoTenantId && x.CommercialPackageId == enterprisePackage.Id && x.DeletedAt == null, cancellationToken);

        if (existingAssignment is not null)
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                update tenant_subscription_packages
                set source = {"Seed"},
                    status = {"Active"},
                    effective_from = {DateTimeOffset.UtcNow.AddYears(-1)},
                    effective_to = null,
                    metadata_json = {"{\"source\":\"demo-seed\",\"email\":\"admin@example.com\"}"}::jsonb,
                    updated_at = now(),
                    deleted_at = null
                where tenant_id = {DemoTenantId}
                  and commercial_package_id = {enterprisePackage.Id};
                """,
                cancellationToken);
            return;
        }

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            insert into tenant_subscription_packages ("Id", tenant_id, commercial_package_id, source, status, effective_from, effective_to, metadata_json, created_at, updated_at, deleted_at)
            values ({Guid.NewGuid()}, {DemoTenantId}, {enterprisePackage.Id}, {"Seed"}, {"Active"}, {DateTimeOffset.UtcNow.AddYears(-1)}, null, {"{\"source\":\"demo-seed\",\"email\":\"admin@example.com\"}"}::jsonb, now(), now(), null);
            """,
            cancellationToken);
    }

    private static async Task BackfillTenantPlanAssignmentsAsync(BillingDbContext dbContext, CancellationToken cancellationToken)
    {
        var packageByPlan = await dbContext.CommercialPackages
            .AsNoTracking()
            .Where(x => x.Code == "legacy.free" || x.Code == "legacy.pro" || x.Code == "legacy.enterprise")
            .ToDictionaryAsync(
                x => x.Code,
                x => x,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken);

        var subscriptions = await dbContext.Subscriptions
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToListAsync(cancellationToken);

        var existingAssignments = await dbContext.TenantSubscriptionPackages
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .Select(x => new { x.TenantId, x.CommercialPackageId })
            .ToListAsync(cancellationToken);

        var assignmentSet = existingAssignments
            .Select(x => (x.TenantId, x.CommercialPackageId))
            .ToHashSet();

        var newAssignments = new List<TenantSubscriptionPackage>();
        foreach (var subscription in subscriptions)
        {
            var packageCode = subscription.PlanType switch
            {
                PlanType.Free => "legacy.free",
                PlanType.Pro => "legacy.pro",
                PlanType.Enterprise => "legacy.enterprise",
                _ => null
            };

            if (packageCode is null || !packageByPlan.TryGetValue(packageCode, out var package))
            {
                continue;
            }

            if (assignmentSet.Contains((subscription.TenantId, package.Id)))
            {
                continue;
            }

            var status = subscription.Status == SubscriptionStatus.Active ? "Active" : subscription.Status.ToString();
            var effectiveTo = subscription.Status == SubscriptionStatus.Cancelled ? subscription.CancelledAt : null;

            newAssignments.Add(TenantSubscriptionPackage.Create(
                subscription.TenantId,
                package.Id,
                "Backfill",
                status,
                subscription.StartDate,
                effectiveTo,
                $"{{\"source\":\"legacy-plan-backfill\",\"subscriptionId\":\"{subscription.Id}\",\"planType\":\"{subscription.PlanType}\"}}"));
        }

        if (newAssignments.Count > 0)
        {
            dbContext.TenantSubscriptionPackages.AddRange(newAssignments);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static string BuildLegacyPricingMetadata(PlanType planType)
    {
        var (monthly, annual) = planType switch
        {
            PlanType.Free => (0m, 0m),
            PlanType.Pro => (49m, 490m),
            PlanType.Enterprise => (199m, 1990m),
            _ => (49m, 490m)
        };

        var payload = new
        {
            pricing = new
            {
                monthly = new { amount = monthly, currency = "USD" },
                annual = new { amount = annual, currency = "USD" }
            },
            taxRate = 0.10m,
            source = "legacy-compatibility"
        };

        return JsonSerializer.Serialize(payload);
    }

    private static IReadOnlyList<CommercialPackageFeature> CreateLegacyFeatures(Guid packageId, PlanType planType)
        => planType switch
        {
            PlanType.Free =>
            [
                CommercialPackageFeature.Create(packageId, "travel.inquiries", false),
                CommercialPackageFeature.Create(packageId, "travel.contacts", false),
                CommercialPackageFeature.Create(packageId, "travel.quotations", false),
                CommercialPackageFeature.Create(packageId, "travel.bookings", false),
                CommercialPackageFeature.Create(packageId, "travel.itineraries", false),
                CommercialPackageFeature.Create(packageId, "travel.followups", false),
                CommercialPackageFeature.Create(packageId, "communication.notifications", false),
                CommercialPackageFeature.Create(packageId, "communication.logs.read", false),
                CommercialPackageFeature.Create(packageId, "communication.templates.manage", false),
                CommercialPackageFeature.Create(packageId, "billing.portal", false),
                CommercialPackageFeature.Create(packageId, "identity.users.manage", false),
                CommercialPackageFeature.Create(packageId, "identity.settings.manage", false),
                CommercialPackageFeature.Create(packageId, "tenant.branding", false),
                CommercialPackageFeature.Create(packageId, "integration.webhooks", false),
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
                CommercialPackageFeature.Create(packageId, "travel.inquiries", true),
                CommercialPackageFeature.Create(packageId, "travel.contacts", true),
                CommercialPackageFeature.Create(packageId, "travel.quotations", true),
                CommercialPackageFeature.Create(packageId, "travel.bookings", true),
                CommercialPackageFeature.Create(packageId, "travel.itineraries", true),
                CommercialPackageFeature.Create(packageId, "travel.followups", true),
                CommercialPackageFeature.Create(packageId, "communication.notifications", true),
                CommercialPackageFeature.Create(packageId, "communication.logs.read", true),
                CommercialPackageFeature.Create(packageId, "communication.templates.manage", true),
                CommercialPackageFeature.Create(packageId, "billing.portal", true),
                CommercialPackageFeature.Create(packageId, "identity.users.manage", true),
                CommercialPackageFeature.Create(packageId, "identity.settings.manage", true),
                CommercialPackageFeature.Create(packageId, "tenant.branding", true),
                CommercialPackageFeature.Create(packageId, "integration.webhooks", true),
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
                CommercialPackageFeature.Create(packageId, "travel.inquiries", true),
                CommercialPackageFeature.Create(packageId, "travel.contacts", true),
                CommercialPackageFeature.Create(packageId, "travel.quotations", true),
                CommercialPackageFeature.Create(packageId, "travel.bookings", true),
                CommercialPackageFeature.Create(packageId, "travel.itineraries", true),
                CommercialPackageFeature.Create(packageId, "travel.followups", true),
                CommercialPackageFeature.Create(packageId, "communication.notifications", true),
                CommercialPackageFeature.Create(packageId, "communication.logs.read", true),
                CommercialPackageFeature.Create(packageId, "communication.templates.manage", true),
                CommercialPackageFeature.Create(packageId, "billing.portal", true),
                CommercialPackageFeature.Create(packageId, "identity.users.manage", true),
                CommercialPackageFeature.Create(packageId, "identity.settings.manage", true),
                CommercialPackageFeature.Create(packageId, "tenant.branding", true),
                CommercialPackageFeature.Create(packageId, "integration.webhooks", true),
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
