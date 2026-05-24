using CommunicationService.Api.Filters;
using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Repositories;
using CommunicationService.Infrastructure.Billing;
using CommunicationService.Infrastructure.Branding;
using CommunicationService.Infrastructure.Caching;
using CommunicationService.Infrastructure.Channels;
using CommunicationService.Infrastructure.Entitlements;
using CommunicationService.Infrastructure.Identity;
using CommunicationService.Infrastructure.Persistence;
using CommunicationService.Infrastructure.Persistence.Outbox;
using CommunicationService.Infrastructure.Persistence.Repositories;
using CommunicationService.Infrastructure.Recipients;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Security.Cryptography;

namespace CommunicationService.Api;

public sealed class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var databaseUrl = builder.Configuration["DATABASE_URL"] ?? "Host=postgres;Port=5432;Database=billing_communication;Username=billing_user;Password=changeme";

        builder.Services.AddDbContext<CommunicationDbContext>(options => options.UseNpgsql(databaseUrl));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantContext, HeaderTenantContext>();
        builder.Services.AddOptions<EmailChannelOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                options.Provider = configuration["EMAIL_PROVIDER"] ?? "log";
                options.DefaultFromEmail = configuration["EMAIL_DEFAULT_FROM_EMAIL"];
                options.DefaultFromName = configuration["EMAIL_DEFAULT_FROM_NAME"];
                options.SendGridApiKey = configuration["SENDGRID_API_KEY"];
                options.SendGridBaseUrl = configuration["SENDGRID_BASE_URL"] ?? "https://api.sendgrid.com/";
            })
            .ValidateOnStart();
        builder.Services.AddSingleton<IValidateOptions<EmailChannelOptions>, EmailChannelOptionsValidator>();
        builder.Services.AddOptions<SmsChannelOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                options.Provider = configuration["SMS_PROVIDER"] ?? "log";
                options.DefaultFromNumber = configuration["SMS_DEFAULT_FROM_NUMBER"];
                options.TwilioAccountSid = configuration["TWILIO_ACCOUNT_SID"];
                options.TwilioAuthToken = configuration["TWILIO_AUTH_TOKEN"];
                options.TwilioBaseUrl = configuration["TWILIO_BASE_URL"] ?? "https://api.twilio.com/";
            })
            .ValidateOnStart();
        builder.Services.AddSingleton<IValidateOptions<SmsChannelOptions>, SmsChannelOptionsValidator>();
        builder.Services.AddOptions<WhatsAppChannelOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                options.Provider = configuration["WHATSAPP_PROVIDER"] ?? "log";
                options.DefaultFromNumber = configuration["WHATSAPP_DEFAULT_FROM_NUMBER"];
                options.TwilioAccountSid = configuration["TWILIO_ACCOUNT_SID"];
                options.TwilioAuthToken = configuration["TWILIO_AUTH_TOKEN"];
                options.TwilioBaseUrl = configuration["TWILIO_BASE_URL"] ?? "https://api.twilio.com/";
            })
            .ValidateOnStart();
        builder.Services.AddSingleton<IValidateOptions<WhatsAppChannelOptions>, WhatsAppChannelOptionsValidator>();
        builder.Services.AddHttpClient<SendGridEmailProvider>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<EmailChannelOptions>>().Value;
            client.BaseAddress = new Uri(options.SendGridBaseUrl);
        });
        builder.Services.AddHttpClient<TwilioSmsProvider>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<SmsChannelOptions>>().Value;
            client.BaseAddress = new Uri(options.TwilioBaseUrl);
        });
        builder.Services.AddHttpClient<TwilioWhatsAppProvider>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<WhatsAppChannelOptions>>().Value;
            client.BaseAddress = new Uri(options.TwilioBaseUrl);
        });
        builder.Services.AddScoped<IEmailDeliveryProvider, LogEmailProvider>();
        builder.Services.AddScoped<IEmailDeliveryProvider>(serviceProvider => serviceProvider.GetRequiredService<SendGridEmailProvider>());
        builder.Services.AddScoped<ISmsDeliveryProvider, LogSmsProvider>();
        builder.Services.AddScoped<ISmsDeliveryProvider>(serviceProvider => serviceProvider.GetRequiredService<TwilioSmsProvider>());
        builder.Services.AddScoped<IWhatsAppDeliveryProvider, LogWhatsAppProvider>();
        builder.Services.AddScoped<IWhatsAppDeliveryProvider>(serviceProvider => serviceProvider.GetRequiredService<TwilioWhatsAppProvider>());
        builder.Services.AddScoped<EmailProviderResolver>();
        builder.Services.AddScoped<SmsProviderResolver>();
        builder.Services.AddScoped<WhatsAppProviderResolver>();
        builder.Services.AddSingleton<ITwilioRequestValidator, TwilioRequestValidator>();
        builder.Services.AddScoped<IChannelPreferenceResolver, ChannelPreferenceResolver>();
        builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
        builder.Services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
        builder.Services.AddScoped<IRecipientPreferencesRepository, RecipientPreferencesRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IReadDbConnectionFactory, ReadDbConnectionFactory>();
        builder.Services.AddScoped<ICacheService, RedisCacheService>();
        builder.Services.AddHttpClient<IIdentityBrandingClient, IdentityBrandingClient>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["IDENTITY_SERVICE_URL"] ?? "http://localhost:8080/");
        });
        builder.Services.AddScoped<IBrandingTemplateRenderer, IdentityBackedBrandingTemplateRenderer>();
        builder.Services.AddHttpClient<IBillingEntitlementsClient, BillingEntitlementsClient>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["BILLING_SERVICE_URL"] ?? "http://localhost:5080/");
        });
        builder.Services.AddHttpClient<IRecipientAddressResolver, TravelContactRecipientAddressResolver>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["TRAVEL_SERVICE_URL"] ?? "http://localhost:5060/");
        });
        builder.Services.AddHttpClient("BillingEventRelayCommunication", client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["COMMUNICATION_SERVICE_URL"] ?? "http://localhost:8080/");
        });
        builder.Services.AddScoped<IFeatureGate, CachedFeatureGate>();
        builder.Services.AddScoped<IChannelDispatcher, EmailDispatcher>();
        builder.Services.AddScoped<IChannelDispatcher, SmsDispatcher>();
        builder.Services.AddScoped<IChannelDispatcher, WhatsAppDispatcher>();
        builder.Services.AddScoped<IChannelDispatcher, PushNotificationDispatcher>();
        builder.Services.AddScoped<IChannelDispatcher, InAppDispatcher>();

        builder.Services.AddStackExchangeRedisCache(options => options.Configuration = builder.Configuration["REDIS_URL"] ?? "redis:6379");
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

        builder.Services.AddHostedService<OutboxPublisherService>();
        builder.Services.AddHostedService<NotificationDispatcherService>();
        builder.Services.AddHostedService<BillingEventRelayService>();
        builder.Services.AddHostedService<PasswordResetRequestedConsumerService>();
        builder.Services.AddHostedService<UserAnonymizedConsumerService>();
        builder.Services.AddHealthChecks();

        ConfigureAuth(builder);

        builder.Services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>());
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CommunicationDbContext>();
            db.Database.Migrate();
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

        builder.Services.AddAuthorization();
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
