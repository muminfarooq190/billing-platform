using System.Text;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace CommunicationService.Infrastructure.Persistence.Outbox;

public sealed class OutboxPublisherService(IServiceScopeFactory scopeFactory, IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { Uri = new Uri(configuration["RABBITMQ_URL"] ?? "amqp://guest:guest@rabbitmq:5672") };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.ExchangeDeclare("communication.events", ExchangeType.Topic, durable: true);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CommunicationDbContext>();
                var messages = await db.DomainEvents.Where(m => m.PublishedAt == null).OrderBy(m => m.CreatedAt).Take(50).ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    var routingKey = $"communication.{message.AggregateType}.{message.EventType}";
                    var body = Encoding.UTF8.GetBytes(message.Payload);
                    channel.BasicPublish("communication.events", routingKey, null, body);
                    message.PublishedAt = DateTimeOffset.UtcNow;
                }

                if (messages.Count > 0) await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception) when (!stoppingToken.IsCancellationRequested) { }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
