using System.Security.Claims;

namespace CommunicationService.Api;

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

            if (!Guid.TryParse(headerValue, out var tenantId))
                throw new InvalidOperationException("Missing or invalid x-tenant-id header.");

            var claimTenantId = httpContext.User.FindFirstValue("tenantId");
            if (!string.IsNullOrWhiteSpace(claimTenantId) && !string.Equals(claimTenantId, tenantId.ToString(), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Tenant header does not match authenticated tenant.");

            return tenantId;
        }
    }

    public Guid? UserId
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context is not available.");
            var raw = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContext.User.FindFirstValue("sub")
                ?? httpContext.Request.Headers["x-user-id"].FirstOrDefault();

            return Guid.TryParse(raw, out var userId) ? userId : null;
        }
    }
}
