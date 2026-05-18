using System.Net.Http.Json;
using System.Text.RegularExpressions;
using ApiGateway.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ApiGateway.Middleware;

public sealed class FeatureEntitlementMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory, IOptions<FeatureEntitlementOptions> options, ILogger<FeatureEntitlementMiddleware> logger, IHostEnvironment environment)
{

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantIdValue = context.Items["tenant_id"] as string;
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            await next(context);
            return;
        }

        var featureKey = MatchFeature(options.Value.Routes, context.Request.Method, context.Request.Path);
        if (featureKey is null)
        {
            await next(context);
            return;
        }

        var entitlements = await GetEntitlementsAsync(tenantId, context.Request.Headers.Authorization.ToString(), context.RequestAborted);
        if (!entitlements.Any(x => string.Equals(x.FeatureKey, featureKey, StringComparison.OrdinalIgnoreCase) && x.Granted))
        {
            logger.LogInformation("Blocked request for tenant {TenantId} due to missing feature {FeatureKey}", tenantId, featureKey);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "feature_not_enabled",
                featureKey,
                message = $"Your current subscription does not include {featureKey}."
            }, context.RequestAborted);
            return;
        }

        await next(context);
    }

    public static string? MatchFeature(IReadOnlyList<FeatureRoutePolicy> routes, string method, PathString path)
    {
        var value = path.Value ?? string.Empty;
        foreach (var route in routes)
        {
            if (!string.Equals(route.Method, method, StringComparison.OrdinalIgnoreCase))
                continue;
            if (string.IsNullOrWhiteSpace(route.FeatureKey))
                continue;

            if (!string.IsNullOrWhiteSpace(route.PathPattern) && PathPatternMatches(route.PathPattern, value))
                return route.FeatureKey;

            if (!string.IsNullOrWhiteSpace(route.PathPrefix) && value.StartsWith(route.PathPrefix, StringComparison.OrdinalIgnoreCase))
                return route.FeatureKey;
        }

        return null;
    }

    private static bool PathPatternMatches(string pattern, string path)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return false;

        var regexPattern = Regex.Replace(pattern.Trim(), "\\{[^/]+\\}", "[^/]+")
            .Replace("**catch-all", ".*");

        regexPattern = $"^{regexPattern}$";
        return Regex.IsMatch(path, regexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private async Task<IReadOnlyList<FeatureEntitlementDto>> GetEntitlementsAsync(Guid tenantId, string authorizationHeader, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("billing-entitlements");

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"billing/entitlements/{tenantId}");
            if (!string.IsNullOrWhiteSpace(authorizationHeader))
            {
                request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
            }
            request.Headers.TryAddWithoutValidation("x-tenant-id", tenantId.ToString());
            using var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var payload = await response.Content.ReadFromJsonAsync<IReadOnlyList<FeatureEntitlementDto>>(cancellationToken: cancellationToken);
            return payload ?? [];
        }
        catch (Exception ex)
        {
            // Fail-open in every environment: if the billing service cannot be
            // reached or returns an unexpected shape, do not block the request
            // (the downstream service still enforces RBAC + per-user feature
            // gates). Previously a non-Development env would re-throw and the
            // outer JWT middleware's try/catch would mis-report it as a 401
            // "invalid_token".
            logger.LogWarning(ex, "Billing entitlement precheck failed for tenant {TenantId}; allowing request to proceed.", tenantId);
            return [];
        }
    }

    internal sealed record FeatureEntitlementDto(string FeatureKey, bool Granted);
}
