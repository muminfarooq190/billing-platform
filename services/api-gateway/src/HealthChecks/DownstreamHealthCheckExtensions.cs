using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiGateway.HealthChecks;

public static class DownstreamHealthCheckExtensions
{
    public static IHealthChecksBuilder AddDownstreamUrl(this IHealthChecksBuilder builder, string name, string healthUrl)
    {
        builder.Services.AddHttpClient($"healthcheck:{name}");

        return builder.Add(new HealthCheckRegistration(
            name,
            serviceProvider => new DownstreamHealthCheck(
                serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient($"healthcheck:{name}"),
                healthUrl),
            HealthStatus.Unhealthy,
            ["ready"]));
    }
}
