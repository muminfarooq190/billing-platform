using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiGateway.HealthChecks;

public sealed class DownstreamHealthCheck(HttpClient httpClient, string healthUrl) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync(healthUrl, cancellationToken);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy($"{healthUrl} is healthy.")
                : HealthCheckResult.Unhealthy($"{healthUrl} returned {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Failed to reach {healthUrl}.", ex);
        }
    }
}
