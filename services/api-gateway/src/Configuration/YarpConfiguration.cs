using System;
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

                    transformContext.ProxyRequest.Headers.Remove("x-tenant-id");
                    transformContext.ProxyRequest.Headers.Remove("x-user-id");

                    if (Guid.TryParse(tenantId, out var parsedTenantId))
                    {
                        transformContext.ProxyRequest.Headers.TryAddWithoutValidation("x-tenant-id", parsedTenantId.ToString());
                    }

                    if (Guid.TryParse(userId, out var parsedUserId))
                    {
                        transformContext.ProxyRequest.Headers.TryAddWithoutValidation("x-user-id", parsedUserId.ToString());
                    }

                    return ValueTask.CompletedTask;
                });
            });

        return services;
    }
}
