using System.Text;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace TravelService.Infrastructure.Persistence.Outbox;

public sealed class OutboxPublisherService(IServiceScopeFactory scopeFactory, IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingMessagesAsync(stoppingToken);
            }
            catch (Exception) when (!stoppingToken.IsCancellationRequested)
            {
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task PublishPendingMessagesAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TravelDbContext>();
        var messages = await db.DomainEvents.Where(m => m.PublishedAt == null).OrderBy(m => m.CreatedAt).Take(50).ToListAsync(stoppingToken);
        if (messages.Count == 0)
        {
            return;
        }

        var factory = new ConnectionFactory { Uri = new Uri(configuration["RABBITMQ_URL"] ?? "amqp://guest:guest@rabbitmq:5672") };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.ExchangeDeclare("travel.events", ExchangeType.Topic, durable: true);

        foreach (var message in messages)
        {
            var body = Encoding.UTF8.GetBytes(message.Payload);
            channel.BasicPublish("travel.events", ResolveRoutingKey(message.EventType), null, body);
            message.PublishedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(stoppingToken);
    }

    private static string ResolveRoutingKey(string eventType) => eventType switch
    {
        "FollowUpCreatedEvent" => "travel.follow-up.created",
        "FollowUpCompletedEvent" => "travel.follow-up.completed",
        "QuotationCreatedEvent" => "travel.quotation.created",
        "QuotationSentEvent" => "travel.quotation.sent",
        "QuotationAcceptedEvent" => "travel.quotation.accepted",
        "ItineraryCreatedEvent" => "travel.itinerary.created",
        "ItineraryConfirmedEvent" => "travel.itinerary.confirmed",
        _ => eventType
    };
}
