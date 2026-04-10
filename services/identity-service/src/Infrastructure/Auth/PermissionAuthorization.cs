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
    public static AuthorizationOptions AddPermissionPolicies(this AuthorizationOptions options)
    {
        var permissions = new[]
        {
            "identity.users.manage",
            "identity.roles.manage",
            "identity.audit.read",
            "identity.settings.manage",
            "branding.theme.manage",
            "identity.tenant.manage"
        };

        foreach (var permission in permissions)
        {
            options.AddPolicy(RequirePermissionAttribute.PolicyPrefix + permission, p => p.Requirements.Add(new PermissionRequirement(permission)));
        }

        return options;
    }
}
