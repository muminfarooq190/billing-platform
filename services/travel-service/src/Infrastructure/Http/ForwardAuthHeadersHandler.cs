using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace TravelService.Infrastructure.Http;

public sealed class ForwardAuthHeadersHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            var authHeader = httpContext.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authHeader) && request.Headers.Authorization is null
                && AuthenticationHeaderValue.TryParse(authHeader, out var parsed))
            {
                request.Headers.Authorization = parsed;
            }

            var tenantHeader = httpContext.Request.Headers["x-tenant-id"].ToString();
            if (!string.IsNullOrWhiteSpace(tenantHeader) && !request.Headers.Contains("x-tenant-id"))
            {
                request.Headers.Add("x-tenant-id", tenantHeader);
            }
        }

        return base.SendAsync(request, cancellationToken);
    }
}
