using ApiGateway.Configuration;
using ApiGateway.Middleware;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(builder.Configuration["REDIS_URL"] ?? "redis:6379"));
builder.Services.AddGatewayReverseProxy(builder.Configuration);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("api-gateway"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter());

var app = builder.Build();

app.UseHttpMetrics();
app.UseMiddleware<JwtValidationMiddleware>();
app.UseMiddleware<RateLimitMiddleware>();

app.MapHealthChecks("/health");
app.MapMetrics("/metrics");
app.MapReverseProxy();

app.Run();
