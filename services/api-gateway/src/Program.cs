using ApiGateway.Configuration;
using ApiGateway.HealthChecks;
using ApiGateway.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks()
    .AddDownstreamUrl("identity-service", "http://identity-service:8080/health")
    .AddDownstreamUrl("billing-service", "http://billing-service:8080/health")
    .AddDownstreamUrl("travel-service", "http://travel-service:8080/health")
    .AddDownstreamUrl("communication-service", "http://communication-service:8080/health")
    .AddDownstreamUrl("webhook-service", "http://webhook-service:3000/health");
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(builder.Configuration["REDIS_URL"] ?? "redis:6379"));
builder.Services.AddHttpClient("billing-entitlements", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BILLING_SERVICE_URL"] ?? "http://billing-service:8080/");
});
builder.Services.AddGatewayReverseProxy(builder.Configuration);

var allowedOrigins = (builder.Configuration["ALLOWED_ORIGINS"] ?? string.Empty)
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendClients", policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();

            return;
        }

        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("api-gateway"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter());

var app = builder.Build();

app.UseHttpMetrics();
app.UseCors("FrontendClients");
app.UseMiddleware<JwtValidationMiddleware>();
app.UseMiddleware<FeatureEntitlementMiddleware>();
app.UseMiddleware<RateLimitMiddleware>();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});
app.MapMetrics("/metrics");
app.MapReverseProxy();

app.Run();
