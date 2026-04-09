using System.Security.Claims;
using TravelService.Application.Abstractions;

namespace TravelService.Api;

public sealed class HttpActorContext(IHttpContextAccessor httpContextAccessor, ITenantContext tenantContext) : IActorContext
{
    public Guid? UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            var claimValue = user?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user?.FindFirstValue("sub")
                ?? user?.FindFirstValue("userId");

            return Guid.TryParse(claimValue, out var userId) ? userId : null;
        }
    }

    public Guid TenantId => tenantContext.TenantId;

    public string? IpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent => httpContextAccessor.HttpContext?.Request.Headers.UserAgent.FirstOrDefault();
}
