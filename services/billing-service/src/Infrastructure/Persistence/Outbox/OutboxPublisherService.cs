using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace BillingService.Infrastructure.Persistence.Outbox;

public sealed class OutboxPublisherService(IServiceScopeFactory scopeFactory, IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await PublishAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task PublishAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
        var pending = await db.DomainEvents.Where(x => x.PublishedAt == null).OrderBy(x => x.CreatedAt).Take(100).ToListAsync(cancellationToken);
        if (pending.Count == 0) return;

        var factory = new ConnectionFactory { Uri = new Uri(configuration["RABBITMQ_URL"] ?? "amqp://guest:guest@rabbitmq:5672") };
        using var conn = factory.CreateConnection();
        using var channel = conn.CreateModel();
        channel.ExchangeDeclare("billing.events", ExchangeType.Topic, durable: true);

        foreach (var message in pending)
        {
            channel.BasicPublish("billing.events", message.EventType, null, System.Text.Encoding.UTF8.GetBytes(message.Payload));
            message.PublishedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
