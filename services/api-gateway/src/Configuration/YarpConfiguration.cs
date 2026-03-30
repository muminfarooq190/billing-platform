using Yarp.ReverseProxy.Transforms;

namespace ApiGateway.Configuration;

public static class YarpConfiguration
{
    public static IServiceCollection AddGatewayReverseProxy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("Yarp:ReverseProxy"))
            .AddTransforms(builderContext =>
            {
                builderContext.AddRequestTransform(transformContext =>
                {
                    var tenantId = transformContext.HttpContext.Items["tenant_id"] as string;
                    var userId = transformContext.HttpContext.Items["user_id"] as string;
                    if (!string.IsNullOrWhiteSpace(tenantId))
                    {
                        transformContext.ProxyRequest.Headers.Add("x-tenant-id", tenantId);
                    }

                    if (!string.IsNullOrWhiteSpace(userId))
                    {
                        transformContext.ProxyRequest.Headers.Add("x-user-id", userId);
                    }

                    return ValueTask.CompletedTask;
                });
            });

        return services;
    }
}
