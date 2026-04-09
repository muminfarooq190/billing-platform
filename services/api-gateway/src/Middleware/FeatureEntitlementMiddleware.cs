using System.Net.Http.Json;

namespace ApiGateway.Middleware;

public sealed class FeatureEntitlementMiddleware(RequestDelegate next, IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<FeatureEntitlementMiddleware> logger)
{
    private static readonly Dictionary<string, string> RouteFeatureMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["POST:/api/travel/quotations"] = "travel.quotation.create",
        ["POST:/api/travel/quotations/send"] = "travel.quotation.send",
        ["POST:/api/travel/bookings/from-quotation"] = "travel.booking.create",
        ["POST:/api/travel/bookings/documents"] = "travel.booking.documents.upload",
        ["GET:/api/travel/timeline"] = "travel.timeline.read",
        ["GET:/api/travel/admin/audit"] = "travel.audit.read",
        ["POST:/api/communication/notifications"] = "communication.notification.send",
        ["POST:/api/communication/templates"] = "communication.templates.manage",
        ["PUT:/api/communication/templates"] = "communication.templates.manage"
    };

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantIdValue = context.Items["tenant_id"] as string;
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            await next(context);
            return;
        }

        var featureKey = MatchFeature(context.Request.Method, context.Request.Path);
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

    public static string? MatchFeature(string method, PathString path)
    {
        var value = path.Value ?? string.Empty;
        foreach (var (prefix, featureKey) in RouteFeatureMap)
        {
            var parts = prefix.Split(':', 2);
            if (!string.Equals(parts[0], method, StringComparison.OrdinalIgnoreCase))
                continue;

            if (value.StartsWith(parts[1], StringComparison.OrdinalIgnoreCase))
                return featureKey;
        }

        return null;
    }

    private async Task<IReadOnlyList<FeatureEntitlementDto>> GetEntitlementsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("billing-entitlements");
        var response = await client.GetFromJsonAsync<IReadOnlyList<FeatureEntitlementDto>>($"billing/entitlements/{tenantId}", cancellationToken);
        return response ?? [];
    }

    internal sealed record FeatureEntitlementDto(string FeatureKey, bool Granted);
}
