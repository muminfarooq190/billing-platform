using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace ApiGateway.Middleware;

public sealed class JwtValidationMiddleware(RequestDelegate next, IConfiguration configuration)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/metrics") ||
            context.Request.Path.StartsWithSegments("/api/auth/register") ||
            context.Request.Path.StartsWithSegments("/api/auth/login") ||
            context.Request.Path.StartsWithSegments("/api/auth/refresh") ||
            context.Request.Path.StartsWithSegments("/api/auth/logout") ||
            context.Request.Path.StartsWithSegments("/.well-known/jwks.json"))
        {
            await next(context);
            return;
        }

        var authorizationHeader = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.WWWAuthenticate = "Bearer realm=\"billing-platform\"";
            return;
        }

        var token = authorizationHeader["Bearer ".Length..].Trim();
        var keyPem = configuration["JWT_PUBLIC_KEY"];
        if (string.IsNullOrWhiteSpace(keyPem))
        {
            var keyPath = configuration["JWT_PUBLIC_KEY_PATH"];
            if (!string.IsNullOrWhiteSpace(keyPath) && File.Exists(keyPath))
            {
                keyPem = File.ReadAllText(keyPath);
            }
        }

        if (string.IsNullOrWhiteSpace(keyPem))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.WWWAuthenticate = "Bearer error=\"invalid_token\"";
            return;
        }

        try
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(keyPem);
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new RsaSecurityKey(rsa),
                NameClaimType = ClaimTypes.NameIdentifier,
                RoleClaimType = ClaimTypes.Role
            }, out _);

            var tenantId = principal.FindFirst("tenant_id")?.Value;
            var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            context.Items["tenant_id"] = tenantId ?? "anonymous";
            context.Items["user_id"] = userId ?? "anonymous";
            context.User = principal;

            await next(context);
        }
        catch
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.WWWAuthenticate = "Bearer error=\"invalid_token\"";
        }
    }
}
