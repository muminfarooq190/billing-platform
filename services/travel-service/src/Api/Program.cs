using TravelService.Api.Filters;
using TravelService.Application.Abstractions;
using TravelService.Domain.Repositories;
using TravelService.Infrastructure.Caching;
using TravelService.Infrastructure.Persistence;
using TravelService.Infrastructure.Persistence.Outbox;
using TravelService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TravelService.Api;

public sealed class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var databaseUrl = builder.Configuration["DATABASE_URL"] ?? "Host=postgres;Port=5432;Database=billing_travel;Username=billing_user;Password=changeme";

        builder.Services.AddDbContext<TravelDbContext>(options => options.UseNpgsql(databaseUrl));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantContext, HeaderTenantContext>();
        builder.Services.AddScoped<IContactRepository, ContactRepository>();
        builder.Services.AddScoped<IFollowUpRepository, FollowUpRepository>();
        builder.Services.AddScoped<IQuotationRepository, QuotationRepository>();
        builder.Services.AddScoped<IQuotationRevisionRepository, QuotationRevisionRepository>();
        builder.Services.AddScoped<IQuotationStatusHistoryRepository, QuotationStatusHistoryRepository>();
        builder.Services.AddScoped<IItineraryRepository, ItineraryRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IReadDbConnectionFactory, ReadDbConnectionFactory>();
        builder.Services.AddScoped<ICacheService, RedisCacheService>();

        builder.Services.AddStackExchangeRedisCache(options => options.Configuration = builder.Configuration["REDIS_URL"] ?? "redis:6379");
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

        builder.Services.AddHostedService<OutboxPublisherService>();
        builder.Services.AddHealthChecks();

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
        app.MapControllers();
        app.MapHealthChecks("/health");
        app.Run();
    }
}
