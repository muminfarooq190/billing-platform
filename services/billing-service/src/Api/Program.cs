using BillingService.Api.Filters;
using BillingService.Application.Abstractions;
using BillingService.Domain.Repositories;
using BillingService.Infrastructure.Caching;
using BillingService.Infrastructure.Entitlements;
using BillingService.Infrastructure.Jobs;
using BillingService.Infrastructure.Payments;
using BillingService.Infrastructure.Persistence;
using BillingService.Infrastructure.Persistence.Outbox;
using BillingService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Api;

public sealed class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var databaseUrl = builder.Configuration["DATABASE_URL"] ?? "Host=postgres;Port=5432;Database=billing_billing;Username=billing_user;Password=changeme";

        builder.Services.AddDbContext<BillingDbContext>(options => options.UseNpgsql(databaseUrl));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantContext, HeaderTenantContext>();
        builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        builder.Services.AddScoped<IFeatureEntitlementRepository, FeatureEntitlementRepository>();
        builder.Services.AddScoped<ICommercialPackageRepository, CommercialPackageRepository>();
        builder.Services.AddScoped<ITenantSubscriptionPackageRepository, TenantSubscriptionPackageRepository>();
        builder.Services.AddScoped<ITenantFeatureOverrideRepository, TenantFeatureOverrideRepository>();
        builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IReadDbConnectionFactory, ReadDbConnectionFactory>();
        builder.Services.AddScoped<ICacheService, RedisCacheService>();

        builder.Services.AddStackExchangeRedisCache(options => options.Configuration = builder.Configuration["REDIS_URL"] ?? "redis:6379");
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

        if (string.Equals(builder.Configuration["PAYMENT_GATEWAY"], "Stripe", StringComparison.OrdinalIgnoreCase))
        {
            builder.Services.AddScoped<IPaymentGateway, StripePaymentGateway>();
        }
        else
        {
            builder.Services.AddScoped<IPaymentGateway, MockPaymentGateway>();
        }

        builder.Services.AddHostedService<OutboxPublisherService>();
        builder.Services.AddHostedService<BillingSchedulerService>();
        builder.Services.AddHostedService<OverdueInvoiceCheckerService>();
        builder.Services.AddHealthChecks();

        builder.Services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>());
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
            db.Database.Migrate();
            BillingSeed.SeedFlexibleEntitlementsAsync(db, CancellationToken.None).GetAwaiter().GetResult();
        }

        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapControllers();
        app.MapHealthChecks("/health");
        app.Run();
    }
}
