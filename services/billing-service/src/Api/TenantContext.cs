using System.Security.Claims;

namespace BillingService.Api;

public interface ITenantContext
{
    Guid TenantId { get; }
    Guid? UserId { get; }
}

public sealed class HeaderTenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    public Guid TenantId
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context is not available.");
            var headerValue = httpContext.Request.Headers["x-tenant-id"].FirstOrDefault();
            var claimTenantId = httpContext.User.FindFirstValue("tenantId")
                ?? httpContext.User.FindFirstValue("tenant_id");

            if (Guid.TryParse(headerValue, out var headerTenantId))
            {
                if (!string.IsNullOrWhiteSpace(claimTenantId) && !string.Equals(claimTenantId, headerTenantId.ToString(), StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Tenant header does not match authenticated tenant.");

                return headerTenantId;
            }

            if (Guid.TryParse(claimTenantId, out var claimTenantGuid))
            {
                return claimTenantGuid;
            }

            throw new InvalidOperationException("Missing tenant context.");
        }
    }

    public Guid? UserId
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context is not available.");
            var raw = httpContext.Request.Headers["x-user-id"].FirstOrDefault();

            if (Guid.TryParse(raw, out var headerUserId))
            {
                return headerUserId;
            }

            raw = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContext.User.FindFirstValue("sub");

            return Guid.TryParse(raw, out var userId) ? userId : null;
        }
    }
}
