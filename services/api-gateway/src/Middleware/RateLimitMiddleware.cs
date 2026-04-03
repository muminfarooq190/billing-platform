using System.Globalization;
using Prometheus;
using StackExchange.Redis;

namespace ApiGateway.Middleware;

public sealed class RateLimitMiddleware(RequestDelegate next, IConnectionMultiplexer redis)
{
    private static readonly Counter RateLimitHits = Metrics.CreateCounter(
        "gateway_rate_limit_hits_total",
        "Number of requests blocked by gateway tenant rate limiting.",
        ["tenant_id"]);

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/health") || context.Request.Path.StartsWithSegments("/metrics"))
        {
            await next(context);
            return;
        }

        var tenantId = context.Items["tenant_id"] as string ?? "anonymous";
        var cache = redis.GetDatabase();
        var overrideRaw = await cache.StringGetAsync($"ratelimit:{tenantId}");
        var limit = 100;
        if (!overrideRaw.IsNullOrEmpty && int.TryParse(overrideRaw.ToString(), CultureInfo.InvariantCulture, out var overrideLimit))
        {
            limit = overrideLimit;
        }

        var windowKey = $"ratelimit:window:{tenantId}:{DateTimeOffset.UtcNow:yyyyMMddHHmm}";
        var current = await cache.StringIncrementAsync(windowKey);
        if (current == 1)
        {
            await cache.KeyExpireAsync(windowKey, TimeSpan.FromSeconds(60));
        }

        if (current > limit)
        {
            RateLimitHits.WithLabels(tenantId).Inc();
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.RetryAfter = "60";
            await context.Response.WriteAsJsonAsync(new { error = "rate_limit_exceeded", tenantId });
            return;
        }

        await next(context);
    }
}
