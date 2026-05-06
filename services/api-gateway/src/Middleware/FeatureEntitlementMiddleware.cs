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

        var entitlements = await GetEntitlementsAsync(tenantId, context.RequestAborted);
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

    private async Task<IReadOnlyList<FeatureEntitlementDto>> GetEntitlementsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("billing-entitlements");

        try
        {
            var response = await client.GetFromJsonAsync<IReadOnlyList<FeatureEntitlementDto>>($"billing/entitlements/{tenantId}", cancellationToken);
            return response ?? [];
        }
        catch when (environment.IsDevelopment())
        {
            logger.LogWarning("Billing entitlement precheck failed for tenant {TenantId}; allowing local/dev fallback.", tenantId);
            return [
                new FeatureEntitlementDto("travel.inquiries.write", true),
                new FeatureEntitlementDto("travel.concepts.write", true),
                new FeatureEntitlementDto("travel.workflowHub", true),
                new FeatureEntitlementDto("travel.timeline.read", true),
                new FeatureEntitlementDto("travel.quotation.create", true),
                new FeatureEntitlementDto("travel.booking.create", true)
            ];
        }
    }

    internal sealed record FeatureEntitlementDto(string FeatureKey, bool Granted);
}
