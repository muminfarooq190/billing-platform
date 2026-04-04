using CommunicationService.Api.Filters;
using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Repositories;
using CommunicationService.Infrastructure.Caching;
using CommunicationService.Infrastructure.Channels;
using CommunicationService.Infrastructure.Persistence;
using CommunicationService.Infrastructure.Persistence.Outbox;
using CommunicationService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CommunicationService.Api;

public sealed class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var databaseUrl = builder.Configuration["DATABASE_URL"] ?? "Host=postgres;Port=5432;Database=billing_communication;Username=billing_user;Password=changeme";

        builder.Services.AddDbContext<CommunicationDbContext>(options => options.UseNpgsql(databaseUrl));
        builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
        builder.Services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
        builder.Services.AddScoped<IRecipientPreferencesRepository, RecipientPreferencesRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IReadDbConnectionFactory, ReadDbConnectionFactory>();
        builder.Services.AddScoped<ICacheService, RedisCacheService>();
        builder.Services.AddScoped<IChannelDispatcher, EmailDispatcher>();
        builder.Services.AddScoped<IChannelDispatcher, SmsDispatcher>();
        builder.Services.AddScoped<IChannelDispatcher, PushNotificationDispatcher>();
        builder.Services.AddScoped<IChannelDispatcher, InAppDispatcher>();

        builder.Services.AddStackExchangeRedisCache(options => options.Configuration = builder.Configuration["REDIS_URL"] ?? "redis:6379");
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

        builder.Services.AddHostedService<OutboxPublisherService>();
        builder.Services.AddHostedService<NotificationDispatcherService>();
        builder.Services.AddHealthChecks();

        builder.Services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>());
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CommunicationDbContext>();
            db.Database.EnsureCreated();
        }

        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapControllers();
        app.MapHealthChecks("/health");
        app.Run();
    }
}
