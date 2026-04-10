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
        foreach (var permission in Permissions.All)
        {
            options.AddPolicy(RequirePermissionAttribute.PolicyPrefix + permission, p => p.Requirements.Add(new PermissionRequirement(permission)));
        }

        return options;
    }
}
