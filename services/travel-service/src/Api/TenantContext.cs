using System.Security.Claims;

namespace TravelService.Api;

public interface ITenantContext
{
    Guid TenantId { get; }
}

public sealed class HeaderTenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    public Guid TenantId
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context is not available.");
            var headerValue = httpContext.Request.Headers["x-tenant-id"].FirstOrDefault();

            if (!Guid.TryParse(headerValue, out var tenantId))
                throw new InvalidOperationException("Missing or invalid x-tenant-id header.");

            var claimTenantId = httpContext.User.FindFirstValue("tenantId");
            if (!string.IsNullOrWhiteSpace(claimTenantId) && !string.Equals(claimTenantId, tenantId.ToString(), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Tenant header does not match authenticated tenant.");

            return tenantId;
        }
    }
}
