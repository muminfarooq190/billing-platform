using System.Linq;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.Enums;
using IdentityService.Domain.ValueObjects;
using IdentityService.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IdentityService.Infrastructure.Persistence;

public static class IdentitySeed
{
    public static readonly Guid DemoTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid DemoUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public const string DemoTenantName = "Voyara";
    public const string DemoEmail = "admin@example.com";
    public const string DemoPassword = "Demo1234!";

    public static async Task SeedDefaultsAsync(IdentityDbContext dbContext, CancellationToken cancellationToken)
    {
        var permissionDefinitions = new (string Key, string Category, string Description)[]
        {
            (Permissions.Identity.UsersManage, "identity", "Manage users and lifecycle."),
            (Permissions.Identity.RolesManage, "identity", "Manage role definitions and assignments."),
            (Permissions.Identity.AuditRead, "identity", "Read identity audit and security events."),
            (Permissions.Identity.SettingsRead, "identity", "Read tenant settings."),
            (Permissions.Identity.SettingsManage, "identity", "Manage tenant settings."),
            (Permissions.Identity.TenantManage, "identity", "Manage tenant profile, plan and suspension controls."),
            (Permissions.Branding.ThemeRead, "branding", "Read tenant branding and theme details."),
            (Permissions.Branding.ThemeManage, "branding", "Manage tenant branding and template themes."),
            (Permissions.Travel.WorkflowHubRead, "travel", "Read workflow hub and travel work queue."),
            (Permissions.Travel.InquiriesRead, "travel", "Read travel inquiries."),
            (Permissions.Travel.InquiriesWrite, "travel", "Create and update travel inquiries."),
            (Permissions.Travel.ContactsRead, "travel", "Read travel contacts."),
            (Permissions.Travel.ContactsWrite, "travel", "Create and update travel contacts."),
            (Permissions.Travel.FollowUpsRead, "travel", "Read travel follow-ups."),
            (Permissions.Travel.FollowUpsWrite, "travel", "Create and update travel follow-ups."),
            (Permissions.Travel.BookingsRead, "travel", "Read travel bookings."),
            (Permissions.Travel.BookingsWrite, "travel", "Create and update travel bookings."),
            (Permissions.Travel.ItinerariesRead, "travel", "Read itineraries."),
            (Permissions.Travel.ItinerariesWrite, "travel", "Create and update itineraries."),
            (Permissions.Travel.TimelineRead, "travel", "Read timeline and work queue."),
            (Permissions.Travel.NotesRead, "travel", "Read travel notes."),
            (Permissions.Travel.NotesWrite, "travel", "Create and update travel notes."),
            (Permissions.Travel.DocumentsRead, "travel", "Read travel documents."),
            (Permissions.Travel.QuotationRead, "travel", "Read quotations."),
            (Permissions.Travel.QuotationsRead, "travel", "Read quotations list and details."),
            (Permissions.Travel.QuotationWrite, "travel", "Create and update quotations."),
            (Permissions.Travel.AuditRead, "travel", "Read travel audit/reporting."),
            (Permissions.Travel.TemplatesRead, "travel", "Read travel templates."),
            (Permissions.Travel.TemplatesWrite, "travel", "Create and update travel templates."),
            (Permissions.Billing.InvoicesRead, "billing", "Read invoices and billing summaries."),
            (Permissions.Communication.LogsRead, "communication", "Read notification logs and communication activity."),
            (Permissions.Communication.NotificationSend, "communication", "Send notifications across supported channels."),
            (Permissions.Communication.TemplatesManage, "communication", "Manage communication templates."),
        };

        var existingPermissionKeys = await dbContext.PermissionDefinitions
            .AsNoTracking()
            .Select(x => x.Key)
            .ToListAsync(cancellationToken);

        var missingPermissions = permissionDefinitions
            .Where(def => !existingPermissionKeys.Contains(def.Key, StringComparer.OrdinalIgnoreCase))
            .Select(def => PermissionDefinition.Create(def.Key, def.Category, def.Description))
            .ToList();

        if (missingPermissions.Count > 0)
        {
            dbContext.PermissionDefinitions.AddRange(missingPermissions);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await UpsertSystemRoleAsync(
            dbContext,
            "Owner",
            "System owner role.",
            Permissions.All,
            cancellationToken);

        await UpsertSystemRoleAsync(
            dbContext,
            "Admin",
            "System admin role.",
            new[]
            {
                Permissions.Identity.UsersManage,
                Permissions.Identity.AuditRead,
                Permissions.Identity.SettingsRead,
                Permissions.Identity.SettingsManage,
                Permissions.Identity.TenantManage,
                Permissions.Branding.ThemeRead,
                Permissions.Branding.ThemeManage,
                Permissions.Travel.WorkflowHubRead,
                Permissions.Travel.InquiriesRead,
                Permissions.Travel.InquiriesWrite,
                Permissions.Travel.ContactsRead,
                Permissions.Travel.ContactsWrite,
                Permissions.Travel.BookingsRead,
                Permissions.Travel.BookingsWrite,
                Permissions.Travel.ItinerariesRead,
                Permissions.Travel.ItinerariesWrite,
                Permissions.Travel.TimelineRead,
                Permissions.Travel.NotesRead,
                Permissions.Travel.NotesWrite,
                Permissions.Travel.DocumentsRead,
                Permissions.Travel.QuotationRead,
                Permissions.Travel.QuotationsRead,
                Permissions.Travel.QuotationWrite,
                Permissions.Travel.TemplatesRead,
                Permissions.Travel.TemplatesWrite,
                Permissions.Billing.InvoicesRead,
                Permissions.Communication.LogsRead,
                Permissions.Communication.NotificationSend,
                Permissions.Communication.TemplatesManage,
            },
            cancellationToken);

        await UpsertSystemRoleAsync(
            dbContext,
            "Member",
            "Standard member role.",
            new[]
            {
                Permissions.Identity.SettingsRead,
                Permissions.Branding.ThemeRead,
                Permissions.Travel.InquiriesRead,
                Permissions.Travel.ContactsRead,
                Permissions.Travel.BookingsRead,
                Permissions.Travel.ItinerariesRead,
                Permissions.Travel.TimelineRead,
                Permissions.Travel.NotesRead,
                Permissions.Travel.DocumentsRead,
                Permissions.Travel.QuotationRead,
                Permissions.Travel.QuotationsRead,
                Permissions.Travel.TemplatesRead,
            },
            cancellationToken);

        await SeedDemoTenantAsync(dbContext, cancellationToken);
    }

    private static async Task SeedDemoTenantAsync(IdentityDbContext dbContext, CancellationToken cancellationToken)
    {
        var ownerRole = await dbContext.RoleDefinitions
            .AsNoTracking()
            .FirstAsync(x => x.TenantId == null && x.NormalizedName == "OWNER", cancellationToken);

        var demoTenant = await dbContext.Tenants.FirstOrDefaultAsync(x => x.Id == DemoTenantId, cancellationToken);
        if (demoTenant is null)
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                insert into tenants ("Id", "Name", "Email", "Plan", "Status", created_at, updated_at, deleted_at)
                values ({DemoTenantId}, {DemoTenantName}, {DemoEmail}, {"Enterprise"}, {"Active"}, now(), now(), null)
                on conflict ("Id") do update
                set "Name" = excluded."Name",
                    "Email" = excluded."Email",
                    "Plan" = excluded."Plan",
                    "Status" = excluded."Status",
                    updated_at = now(),
                    deleted_at = null;
                """,
                cancellationToken);

            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                update tenants
                set "Name" = {DemoTenantName},
                    "Email" = {DemoEmail},
                    "Plan" = {"Enterprise"},
                    "Status" = {"Active"},
                    updated_at = now(),
                    deleted_at = null
                where "Id" = {DemoTenantId};
                """,
                cancellationToken);

            demoTenant = await dbContext.Tenants.FirstAsync(x => x.Id == DemoTenantId, cancellationToken);
        }

        var demoUser = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == DemoUserId, cancellationToken);
        if (demoUser is null)
        {
            demoUser = User.Create(new TenantId(DemoTenantId), new Email(DemoEmail), BCrypt.Net.BCrypt.HashPassword(DemoPassword), UserRole.Owner);
            await dbContext.Users.AddAsync(demoUser, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"update users set \"Id\" = {DemoUserId}, \"TenantId\" = {DemoTenantId}, \"created_at\" = now(), \"updated_at\" = now(), \"PasswordHash\" = {BCrypt.Net.BCrypt.HashPassword(DemoPassword)}, \"Role\" = {UserRole.Owner.ToString()}, \"Status\" = {UserStatus.Active.ToString()}, must_change_password = false, deleted_at = null where \"Id\" = {demoUser.Id};",
                cancellationToken);

            demoUser = await dbContext.Users.FirstAsync(x => x.Id == DemoUserId, cancellationToken);
        }
        else
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"update users set \"TenantId\" = {DemoTenantId}, \"Email\" = {DemoEmail}, \"PasswordHash\" = {BCrypt.Net.BCrypt.HashPassword(DemoPassword)}, \"Role\" = {UserRole.Owner.ToString()}, \"Status\" = {UserStatus.Active.ToString()}, must_change_password = false, deleted_at = null, \"updated_at\" = now() where \"Id\" = {DemoUserId};",
                cancellationToken);
        }

        var hasAssignment = await dbContext.UserRoleAssignments
            .AnyAsync(x => x.TenantId == DemoTenantId && x.UserId == DemoUserId && x.RoleDefinitionId == ownerRole.Id, cancellationToken);

        if (!hasAssignment)
        {
            dbContext.UserRoleAssignments.Add(UserRoleAssignment.Create(DemoTenantId, DemoUserId, ownerRole.Id));
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task UpsertSystemRoleAsync(
        IdentityDbContext dbContext,
        string name,
        string description,
        IEnumerable<string> permissionKeys,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToUpperInvariant();
        var role = await dbContext.RoleDefinitions
            .FirstOrDefaultAsync(x => x.TenantId == null && x.NormalizedName == normalizedName, cancellationToken);

        if (role is null)
        {
            role = RoleDefinition.Create(null, name, description, true);
            dbContext.RoleDefinitions.Add(role);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var existingAssignments = await dbContext.RolePermissionAssignments
            .Where(x => x.RoleDefinitionId == role.Id)
            .ToListAsync(cancellationToken);

        if (existingAssignments.Count > 0)
        {
            dbContext.RolePermissionAssignments.RemoveRange(existingAssignments);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var newAssignments = permissionKeys
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(permissionKey => RolePermissionAssignment.Create(role.Id, permissionKey))
            .ToList();

        dbContext.RolePermissionAssignments.AddRange(newAssignments);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
