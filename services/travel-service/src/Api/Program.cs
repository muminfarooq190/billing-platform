using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using TravelService.Api.Auth;
using TravelService.Api.Documents;
using TravelService.Api.Filters;
using TravelService.Application.Abstractions;
using TravelService.Domain.Repositories;
using TravelService.Infrastructure.Billing;
using TravelService.Infrastructure.Caching;
using TravelService.Infrastructure.Communication;
using TravelService.Infrastructure.Entitlements;
using TravelService.Infrastructure.Files;
using TravelService.Infrastructure.Http;
using TravelService.Infrastructure.Persistence;
using TravelService.Infrastructure.Persistence.Outbox;
using TravelService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace TravelService.Api;

public sealed class Program
{
    public static void Main(string[] args)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var builder = WebApplication.CreateBuilder(args);
        var databaseUrl = builder.Configuration["DATABASE_URL"] ?? "Host=postgres;Port=5432;Database=billing_travel;Username=billing_user;Password=changeme";

        builder.Services.AddDbContext<TravelDbContext>(options => options.UseNpgsql(databaseUrl));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantContext, HeaderTenantContext>();
        builder.Services.AddScoped<IPublicTenantResolver, HeaderPublicTenantResolver>();
        builder.Services.AddScoped<IActorContext, HttpActorContext>();
        builder.Services.AddScoped<IActivityEntryRepository, ActivityEntryRepository>();
        builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        builder.Services.AddScoped<IEntityNoteRepository, EntityNoteRepository>();
        builder.Services.AddScoped<IActivityWriter, ActivityWriter>();
        builder.Services.AddScoped<IAuditWriter, AuditWriter>();
        builder.Services.AddScoped<IContactRepository, ContactRepository>();
        builder.Services.AddScoped<IFollowUpRepository, FollowUpRepository>();
        builder.Services.AddScoped<ITravelInquiryRepository, TravelInquiryRepository>();
        builder.Services.AddScoped<ITravelInquiryStatusHistoryRepository, TravelInquiryStatusHistoryRepository>();
        builder.Services.AddScoped<IQuotationRepository, QuotationRepository>();
        builder.Services.AddScoped<IQuotationRevisionRepository, QuotationRevisionRepository>();
        builder.Services.AddScoped<IQuotationAttachmentRepository, QuotationAttachmentRepository>();
        builder.Services.AddScoped<IQuotationStatusHistoryRepository, QuotationStatusHistoryRepository>();
        builder.Services.AddScoped<IQuotationShareLinkRepository, QuotationShareLinkRepository>();
        builder.Services.AddScoped<IQuotationApprovalRequestRepository, QuotationApprovalRequestRepository>();
        builder.Services.AddScoped<IBookingChangeRequestRepository, BookingChangeRequestRepository>();
        builder.Services.AddScoped<IBookingRepository, BookingRepository>();
        builder.Services.AddScoped<IBookingStatusHistoryRepository, BookingStatusHistoryRepository>();
        builder.Services.AddScoped<ITravelerRepository, TravelerRepository>();
        builder.Services.AddScoped<IBookingItemRepository, BookingItemRepository>();
        builder.Services.AddScoped<IBookingDocumentRepository, BookingDocumentRepository>();
        builder.Services.AddScoped<IBookingPaymentRepository, BookingPaymentRepository>();
        builder.Services.AddScoped<IItineraryRepository, ItineraryRepository>();
        builder.Services.AddScoped<IDraftTripConceptRepository, DraftTripConceptRepository>();
        builder.Services.AddScoped<ITravelTemplateRepository, TravelTemplateRepository>();
        builder.Services.AddScoped<ITenantActiveTemplateRepository, TenantActiveTemplateRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IReadDbConnectionFactory, ReadDbConnectionFactory>();
        builder.Services.AddScoped<IFileStorage, LocalFileStorage>();
        builder.Services.AddScoped<ICacheService, RedisCacheService>();
        builder.Services.AddTransient<ForwardAuthHeadersHandler>();
        builder.Services.AddHttpClient<IBillingFinanceClient, BillingFinanceClient>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["BILLING_SERVICE_URL"] ?? "http://localhost:5080/");
        }).AddHttpMessageHandler<ForwardAuthHeadersHandler>();
        builder.Services.AddHttpClient<IBillingEntitlementsClient, BillingEntitlementsClient>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["BILLING_SERVICE_URL"] ?? "http://localhost:5080/");
        }).AddHttpMessageHandler<ForwardAuthHeadersHandler>();
        builder.Services.AddHttpClient<ICommunicationWorkflowClient, CommunicationWorkflowClient>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["COMMUNICATION_SERVICE_URL"] ?? "http://localhost:8080/");
        });
        builder.Services.AddHttpClient<IPublicBrandingClient, PublicBrandingClient>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["IDENTITY_SERVICE_URL"] ?? "http://localhost:5090/");
        });
        builder.Services.AddScoped<IFeatureGate, CachedFeatureGate>();

        builder.Services.AddStackExchangeRedisCache(options => options.Configuration = builder.Configuration["REDIS_URL"] ?? "redis:6379");
        builder.Services.AddScoped<IPdfDocumentRenderer, QuestPdfDocumentRenderer>();
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

        builder.Services.AddHostedService<OutboxPublisherService>();
        builder.Services.AddHealthChecks();

        ConfigureJwtCompatibility(builder);

        builder.Services.AddAuthorization(options => options.AddPermissionPolicies());
        builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        builder.Services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>());
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TravelDbContext>();
            db.Database.Migrate();
        }

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecks("/health");
        app.Run();
    }

    private static void ConfigureJwtCompatibility(WebApplicationBuilder builder)
    {
        var inlinePem = builder.Configuration["JWT_PUBLIC_KEY"];
        if (!string.IsNullOrWhiteSpace(inlinePem))
        {
            return;
        }

        var pemPath = builder.Configuration["JWT_PUBLIC_KEY_PATH"];
        if (!string.IsNullOrWhiteSpace(pemPath) && File.Exists(pemPath))
        {
            builder.Configuration["JWT_PUBLIC_KEY"] = File.ReadAllText(pemPath);
        }
    }
}
