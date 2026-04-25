using IdentityService.Domain.Aggregates;
using IdentityService.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence;

public static class IdentitySeed
{
    public static async Task SeedDefaultsAsync(IdentityDbContext dbContext, CancellationToken cancellationToken)
    {
        if (!await dbContext.PermissionDefinitions.AnyAsync(cancellationToken))
        {
            var permissions = new[]
            {
                PermissionDefinition.Create(Permissions.Identity.UsersManage, "identity", "Manage users and lifecycle."),
                PermissionDefinition.Create(Permissions.Identity.RolesManage, "identity", "Manage role definitions and assignments."),
                PermissionDefinition.Create(Permissions.Identity.AuditRead, "identity", "Read identity audit and security events."),
                PermissionDefinition.Create(Permissions.Identity.SettingsManage, "identity", "Manage tenant settings."),
                PermissionDefinition.Create(Permissions.Identity.TenantManage, "identity", "Manage tenant profile, plan and suspension controls."),
                PermissionDefinition.Create(Permissions.Branding.ThemeManage, "branding", "Manage tenant branding and template themes."),
                PermissionDefinition.Create(Permissions.Travel.QuotationRead, "travel", "Read quotations."),
                PermissionDefinition.Create(Permissions.Travel.QuotationWrite, "travel", "Create and update quotations."),
                PermissionDefinition.Create(Permissions.Billing.InvoicesRead, "billing", "Read invoices and billing summaries."),
                PermissionDefinition.Create(Permissions.Communication.LogsRead, "communication", "Read notification logs and communication activity."),
                PermissionDefinition.Create(Permissions.Communication.NotificationSend, "communication", "Send notifications across supported channels."),
                PermissionDefinition.Create(Permissions.Communication.TemplatesManage, "communication", "Manage communication templates.")
            };
            dbContext.PermissionDefinitions.AddRange(permissions);
        }

        if (!await dbContext.RoleDefinitions.AnyAsync(x => x.TenantId == null, cancellationToken))
        {
            var owner = RoleDefinition.Create(null, "Owner", "System owner role.", true);
            owner.SetPermissions(new[] { Permissions.Identity.UsersManage, Permissions.Identity.RolesManage, Permissions.Identity.AuditRead, Permissions.Identity.SettingsManage, Permissions.Identity.TenantManage, Permissions.Branding.ThemeManage, Permissions.Travel.QuotationRead, Permissions.Travel.QuotationWrite, Permissions.Billing.InvoicesRead, Permissions.Communication.LogsRead, Permissions.Communication.NotificationSend, Permissions.Communication.TemplatesManage });
            var admin = RoleDefinition.Create(null, "Admin", "System admin role.", true);
            admin.SetPermissions(new[] { Permissions.Identity.UsersManage, Permissions.Identity.AuditRead, Permissions.Identity.SettingsManage, Permissions.Identity.TenantManage, Permissions.Branding.ThemeManage, Permissions.Travel.QuotationRead, Permissions.Travel.QuotationWrite, Permissions.Billing.InvoicesRead, Permissions.Communication.LogsRead, Permissions.Communication.NotificationSend, Permissions.Communication.TemplatesManage });
            var member = RoleDefinition.Create(null, "Member", "Standard member role.", true);
            member.SetPermissions(new[] { Permissions.Travel.QuotationRead });
            dbContext.RoleDefinitions.AddRange(owner, admin, member);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
