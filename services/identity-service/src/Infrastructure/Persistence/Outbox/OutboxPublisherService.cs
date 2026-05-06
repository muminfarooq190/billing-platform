using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace IdentityService.Infrastructure.Persistence.Outbox;

public sealed class OutboxPublisherService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxPublisherService> logger,
    IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingMessagesAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed while publishing outbox messages.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task PublishPendingMessagesAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        var pendingMessages = await dbContext.DomainEvents
            .Where(x => x.PublishedAt == null)
            .OrderBy(x => x.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (pendingMessages.Count == 0)
        {
            return;
        }

        var rabbitUrl = configuration["RABBITMQ_URL"] ?? "amqp://guest:guest@rabbitmq:5672";
        var factory = new ConnectionFactory { Uri = new Uri(rabbitUrl) };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.ExchangeDeclare("identity.events", ExchangeType.Topic, durable: true);

        foreach (var message in pendingMessages)
        {
            var body = System.Text.Encoding.UTF8.GetBytes(message.Payload);
            channel.BasicPublish("identity.events", routingKey: ResolveRoutingKey(message.EventType), basicProperties: null, body: body);
            message.PublishedAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string ResolveRoutingKey(string eventType) => eventType switch
    {
        "TenantCreatedEvent" => "identity.tenant.created",
        "TenantSuspendedEvent" => "identity.tenant.suspended",
        "UserCreatedEvent" => "identity.user.created",
        "UserPasswordChangedEvent" => "identity.user.password.changed",
        "UserPasswordResetRequestedEvent" => "identity.user.password-reset-requested",
        _ => eventType
    };
}
