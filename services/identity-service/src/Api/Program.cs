using IdentityService.Api.Filters;
using IdentityService.Application.Abstractions;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Auth;
using IdentityService.Infrastructure.Caching;
using IdentityService.Infrastructure.Entitlements;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Persistence.Outbox;
using IdentityService.Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.IO;

namespace IdentityService.Api;

public sealed class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var databaseUrl = builder.Configuration["DATABASE_URL"] ?? "Host=postgres;Port=5432;Database=billing_identity;Username=billing_user;Password=changeme";
        builder.Services.AddDbContext<IdentityDbContext>(options => options.UseNpgsql(databaseUrl));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantContext, HeaderTenantContext>();

        builder.Services.AddScoped<ITenantRepository, TenantRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IReadDbConnectionFactory, ReadDbConnectionFactory>();
        builder.Services.AddScoped<IBrandAssetStorage, LocalBrandAssetStorage>();
        builder.Services.AddScoped<ICacheService, RedisCacheService>();
        builder.Services.AddScoped<IFeatureGate, CachedFeatureGate>();
        builder.Services.AddSingleton<JwtTokenService>();
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(builder.Configuration["REDIS_URL"] ?? "redis:6379"));
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration["REDIS_URL"] ?? "redis:6379";
        });
        builder.Services.AddScoped<RefreshTokenService>();
        builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        builder.Services.AddHttpClient<IBillingEntitlementsClient, BillingEntitlementsClient>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["BILLING_SERVICE_URL"] ?? "http://billing-service:8080/");
        });

        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IUnitOfWork).Assembly));

        builder.Services.AddHostedService<OutboxPublisherService>();
        builder.Services.AddHealthChecks();

        builder.Services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>());
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        ConfigureAuth(builder);

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            db.Database.Migrate();
            IdentitySeed.SeedDefaultsAsync(db, CancellationToken.None).GetAwaiter().GetResult();
        }

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        app.Run();
    }

    private static void ConfigureAuth(WebApplicationBuilder builder)
    {
        var publicKeyPem = ResolvePublicKeyPem(builder.Configuration);
        if (string.IsNullOrWhiteSpace(publicKeyPem))
        {
            return;
        }

        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        var securityKey = new RsaSecurityKey(rsa);

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey
                };
            });

        builder.Services.AddAuthorization(options => options.AddPermissionPolicies());
    }

    private static string? ResolvePublicKeyPem(IConfiguration configuration)
    {
        var inlinePem = configuration["JWT_PUBLIC_KEY"];
        if (!string.IsNullOrWhiteSpace(inlinePem))
        {
            if (LooksLikeCompletePem(inlinePem))
            {
                return inlinePem;
            }

            if (File.Exists(inlinePem))
            {
                return File.ReadAllText(inlinePem);
            }
        }

        var pemPath = configuration["JWT_PUBLIC_KEY_PATH"];
        if (!string.IsNullOrWhiteSpace(pemPath) && File.Exists(pemPath))
        {
            return File.ReadAllText(pemPath);
        }

        return null;
    }

    private static bool LooksLikeCompletePem(string value)
    {
        return (value.Contains("-----BEGIN PUBLIC KEY-----", StringComparison.Ordinal)
                && value.Contains("-----END PUBLIC KEY-----", StringComparison.Ordinal))
            || (value.Contains("-----BEGIN RSA PUBLIC KEY-----", StringComparison.Ordinal)
                && value.Contains("-----END RSA PUBLIC KEY-----", StringComparison.Ordinal));
    }
}
