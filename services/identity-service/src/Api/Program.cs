using Dapper;
using IdentityService.Api.Filters;
using IdentityService.Application.Abstractions;
using System.Data;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Auth;
using IdentityService.Infrastructure.Caching;
using IdentityService.Infrastructure.Entitlements;
using IdentityService.Infrastructure.Http;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Persistence.Outbox;
using IdentityService.Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.IO;
using System.Threading.RateLimiting;

namespace IdentityService.Api;

public sealed class Program
{
    public static void Main(string[] args)
    {
        // Npgsql 8 returns System.DateTime for timestamptz columns. Register a Dapper
        // TypeHandler so DateTimeOffset properties on read-models bind correctly.
        SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
        SqlMapper.AddTypeHandler(new NullableDateTimeOffsetHandler());

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
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<ForwardAuthHeadersHandler>();
        builder.Services.AddHttpClient<IBillingEntitlementsClient, BillingEntitlementsClient>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["BILLING_SERVICE_URL"] ?? "http://billing-service:8080/");
        }).AddHttpMessageHandler<ForwardAuthHeadersHandler>();

        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IUnitOfWork).Assembly));

        builder.Services.AddHostedService<OutboxPublisherService>();
        builder.Services.AddHealthChecks();

        builder.Services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>());
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        ConfigureRateLimiting(builder);
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
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        app.Run();
    }

    /// <summary>
    /// Rate-limit the unauthenticated auth surface to slow credential
    /// stuffing / password-reset spam.
    ///
    /// Two named policies are exposed:
    ///   auth-login   → 10 requests/min/IP+email (login form, accommodates retries on typos)
    ///   auth-strict  → 3  requests/min/IP+email (forgot-password, reset-password — abuse-prone)
    ///
    /// We partition by `IP + email` (or IP alone when no email) so a shared
    /// office NAT can't lock everyone out by exhausting one user's bucket.
    /// 429 responses include `Retry-After` per spec.
    ///
    /// In-memory only — sufficient for single-instance dev; production
    /// horizontal-scale needs a Redis-backed partitioner. Documented as
    /// follow-up in the MVP audit.
    /// </summary>
    private static void ConfigureRateLimiting(WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, token) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                }
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new { error = "Too many attempts. Take a breath and try again in a minute." }, token);
            };

            options.AddPolicy("auth-login", httpContext => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: BuildAuthPartitionKey(httpContext, fallback: "anon"),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true,
                }));

            options.AddPolicy("auth-strict", httpContext => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: BuildAuthPartitionKey(httpContext, fallback: "anon"),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 3,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true,
                }));
        });
    }

    /// <summary>
    /// Build the rate-limiter partition key. Format: <c>{ip}|{email}</c>.
    /// Email is sniffed from the JSON body for `login`/`forgot-password`/
    /// `reset-password`; falls back to the IP alone when absent.
    /// Body is buffered so MVC can re-read it in the action method.
    /// </summary>
    private static string BuildAuthPartitionKey(HttpContext httpContext, string fallback)
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? fallback;
        var email = TrySniffEmailFromBody(httpContext);
        return string.IsNullOrWhiteSpace(email) ? ip : $"{ip}|{email.ToLowerInvariant()}";
    }

    private static string? TrySniffEmailFromBody(HttpContext httpContext)
    {
        try
        {
            if (!httpContext.Request.HasJsonContentType()) return null;
            httpContext.Request.EnableBuffering();
            var body = httpContext.Request.Body;
            body.Position = 0;
            using var reader = new StreamReader(body, leaveOpen: true);
            var raw = reader.ReadToEnd();
            body.Position = 0;
            if (string.IsNullOrWhiteSpace(raw)) return null;
            using var doc = System.Text.Json.JsonDocument.Parse(raw);
            if (doc.RootElement.ValueKind != System.Text.Json.JsonValueKind.Object) return null;
            if (doc.RootElement.TryGetProperty("email", out var email) && email.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                return email.GetString();
            }
        }
        catch
        {
            // Don't break the request path for a partition-key sniff.
        }
        return null;
    }

    private static void ConfigureAuth(WebApplicationBuilder builder)
    {
        var publicKeyPem = ResolvePublicKeyPem(builder.Configuration);
        if (string.IsNullOrWhiteSpace(publicKeyPem))
        {
            return;
        }

        // Detach RSA parameters so the SecurityKey survives RSA disposal at end of scope.
        // Without ExportParameters, JwtBearerHandler hits ObjectDisposedException on every
        // request and rejects the token with 401 "Bearer was not authenticated".
        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        var securityKey = new RsaSecurityKey(rsa.ExportParameters(false));

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

internal sealed class DateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
{
    public override DateTimeOffset Parse(object value) => value switch
    {
        DateTimeOffset dto => dto,
        DateTime dt => DateTime.SpecifyKind(dt, DateTimeKind.Utc),
        _ => throw new DataException($"Cannot convert {value?.GetType().FullName ?? "null"} to DateTimeOffset.")
    };

    public override void SetValue(IDbDataParameter parameter, DateTimeOffset value) => parameter.Value = value;
}

internal sealed class NullableDateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset?>
{
    public override DateTimeOffset? Parse(object value) => value switch
    {
        null => null,
        DBNull => null,
        DateTimeOffset dto => dto,
        DateTime dt => DateTime.SpecifyKind(dt, DateTimeKind.Utc),
        _ => throw new DataException($"Cannot convert {value.GetType().FullName} to DateTimeOffset?.")
    };

    public override void SetValue(IDbDataParameter parameter, DateTimeOffset? value)
        => parameter.Value = value.HasValue ? value.Value : DBNull.Value;
}
