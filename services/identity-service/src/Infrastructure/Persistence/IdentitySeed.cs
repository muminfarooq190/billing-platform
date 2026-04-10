using IdentityService.Domain.Aggregates;
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
                PermissionDefinition.Create("identity.users.manage", "identity", "Manage users and lifecycle."),
                PermissionDefinition.Create("identity.roles.manage", "identity", "Manage role definitions and assignments."),
                PermissionDefinition.Create("identity.audit.read", "identity", "Read identity audit and security events."),
                PermissionDefinition.Create("identity.settings.manage", "identity", "Manage tenant settings."),
                PermissionDefinition.Create("identity.tenant.manage", "identity", "Manage tenant profile, plan and suspension controls."),
                PermissionDefinition.Create("branding.theme.manage", "branding", "Manage tenant branding and template themes."),
                PermissionDefinition.Create("travel.quotation.read", "travel", "Read quotations."),
                PermissionDefinition.Create("travel.quotation.write", "travel", "Create and update quotations."),
                PermissionDefinition.Create("billing.invoices.read", "billing", "Read invoices and billing summaries.")
            };
            dbContext.PermissionDefinitions.AddRange(permissions);
        }

        if (!await dbContext.RoleDefinitions.AnyAsync(x => x.TenantId == null, cancellationToken))
        {
            var owner = RoleDefinition.Create(null, "Owner", "System owner role.", true);
            owner.SetPermissions(new[] { "identity.users.manage", "identity.roles.manage", "identity.audit.read", "identity.settings.manage", "identity.tenant.manage", "branding.theme.manage", "travel.quotation.read", "travel.quotation.write", "billing.invoices.read" });
            var admin = RoleDefinition.Create(null, "Admin", "System admin role.", true);
            admin.SetPermissions(new[] { "identity.users.manage", "identity.audit.read", "identity.settings.manage", "identity.tenant.manage", "branding.theme.manage", "travel.quotation.read", "travel.quotation.write", "billing.invoices.read" });
            var member = RoleDefinition.Create(null, "Member", "Standard member role.", true);
            member.SetPermissions(new[] { "travel.quotation.read" });
            dbContext.RoleDefinitions.AddRange(owner, admin, member);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
