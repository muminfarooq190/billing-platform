using IdentityService.Domain.Aggregates;
using IdentityService.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence;

public static class IdentitySeed
{
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
            (Permissions.Travel.QuotationRead, "travel", "Read quotations."),
            (Permissions.Travel.QuotationWrite, "travel", "Create and update quotations."),
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
            new[]
            {
                Permissions.Identity.UsersManage,
                Permissions.Identity.RolesManage,
                Permissions.Identity.AuditRead,
                Permissions.Identity.SettingsRead,
                Permissions.Identity.SettingsManage,
                Permissions.Identity.TenantManage,
                Permissions.Branding.ThemeRead,
                Permissions.Branding.ThemeManage,
                Permissions.Travel.QuotationRead,
                Permissions.Travel.QuotationWrite,
                Permissions.Billing.InvoicesRead,
                Permissions.Communication.LogsRead,
                Permissions.Communication.NotificationSend,
                Permissions.Communication.TemplatesManage,
            },
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
                Permissions.Travel.QuotationRead,
                Permissions.Travel.QuotationWrite,
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
                Permissions.Travel.QuotationRead,
            },
            cancellationToken);
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
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.TenantId == null && x.NormalizedName == normalizedName, cancellationToken);

        if (role is null)
        {
            role = RoleDefinition.Create(null, name, description, true);
            role.SetPermissions(permissionKeys);
            dbContext.RoleDefinitions.Add(role);
        }
        else
        {
            role.SetPermissions(permissionKeys);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
