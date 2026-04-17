namespace TravelService.Api;

public interface IPublicTenantResolver
{
    Guid ResolveTenantId();
}

public sealed class HeaderPublicTenantResolver(IHttpContextAccessor httpContextAccessor) : IPublicTenantResolver
{
    public Guid ResolveTenantId()
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context is not available.");

        var tenantHeader = httpContext.Request.Headers["x-public-tenant-id"].FirstOrDefault();
        if (Guid.TryParse(tenantHeader, out var tenantId))
            return tenantId;

        throw new InvalidOperationException("Missing or invalid public tenant resolution header.");
    }
}
