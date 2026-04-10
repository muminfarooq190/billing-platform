using Microsoft.AspNetCore.Authorization;

namespace IdentityService.Infrastructure.Auth;

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var granted = context.User.Claims
            .Where(x => x.Type == "permission")
            .Select(x => x.Value)
            .Contains(requirement.Permission, StringComparer.OrdinalIgnoreCase);

        if (granted)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public static class PermissionPolicies
{
    public const string UsersManage = "permission:identity.users.manage";
    public const string RolesManage = "permission:identity.roles.manage";
    public const string AuditRead = "permission:identity.audit.read";
    public const string SettingsManage = "permission:identity.settings.manage";
    public const string BrandingManage = "permission:branding.theme.manage";
    public const string TenantAdmin = "permission:identity.tenant.manage";

    public static AuthorizationOptions AddPermissionPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(UsersManage, p => p.Requirements.Add(new PermissionRequirement("identity.users.manage")));
        options.AddPolicy(RolesManage, p => p.Requirements.Add(new PermissionRequirement("identity.roles.manage")));
        options.AddPolicy(AuditRead, p => p.Requirements.Add(new PermissionRequirement("identity.audit.read")));
        options.AddPolicy(SettingsManage, p => p.Requirements.Add(new PermissionRequirement("identity.settings.manage")));
        options.AddPolicy(BrandingManage, p => p.Requirements.Add(new PermissionRequirement("branding.theme.manage")));
        options.AddPolicy(TenantAdmin, p => p.Requirements.Add(new PermissionRequirement("identity.tenant.manage")));
        return options;
    }
}
