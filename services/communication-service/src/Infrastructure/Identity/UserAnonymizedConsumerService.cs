using System.Text;
using System.Text.Json;
using CommunicationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommunicationService.Infrastructure.Identity;

/// <summary>
/// Consumes <c>identity.user.anonymized</c> from the identity-service topic
/// exchange. Strips communication-service PII tied to the anonymized user:
///   - <c>RecipientPreferences</c> rows are deleted outright (Email / Phone /
///     DeviceToken are pure PII with no audit value).
///   - <c>Notification</c> rows have Subject / Body / DocumentReferencesJson
///     / MetadataJson blanked but the row is retained for delivery audit.
///
/// Notification body retention exists because regulators (and litigants)
/// sometimes ask "did we ever send this user a message about X" — we keep
/// the metadata-stripped row so the answer remains "yes/no, on this date,
/// via this channel". Email subject + body are PII so they go.
/// </summary>
public sealed class UserAnonymizedConsumerService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<UserAnonymizedConsumerService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rabbitUrl = configuration["RABBITMQ_URL"] ?? "amqp://guest:guest@rabbitmq:5672";
        var factory = new ConnectionFactory { Uri = new Uri(rabbitUrl) };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();
                channel.ExchangeDeclare("identity.events", ExchangeType.Topic, durable: true);
                channel.QueueDeclare("communication.user-anonymized", durable: true, exclusive: false, autoDelete: false);
                channel.QueueBind("communication.user-anonymized", "identity.events", "identity.user.anonymized");
                channel.BasicQos(0, 10, false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (_, args) =>
                {
                    try
                    {
                        var raw = Encoding.UTF8.GetString(args.Body.ToArray());
                        var model = JsonSerializer.Deserialize<UserAnonymizedMessage>(raw, JsonOptions);
                        if (model is null || model.UserId == Guid.Empty)
                        {
                            channel.BasicAck(args.DeliveryTag, false);
                            return;
                        }

                        using var scope = scopeFactory.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<CommunicationDbContext>();
                        AnonymizeAsync(db, model.UserId, stoppingToken).GetAwaiter().GetResult();

                        channel.BasicAck(args.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to process UserAnonymized event.");
                        channel.BasicNack(args.DeliveryTag, false, requeue: true);
                    }
                };

                channel.BasicConsume("communication.user-anonymized", autoAck: false, consumer: consumer);

                while (!stoppingToken.IsCancellationRequested && connection.IsOpen)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "UserAnonymized consumer crashed; retrying.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private static async Task AnonymizeAsync(CommunicationDbContext db, Guid userId, CancellationToken cancellationToken)
    {
        // Drop preferences outright — pure PII, no audit value.
        await db.RecipientPreferences
            .Where(x => x.RecipientId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        // Blank notification PII fields but keep the row for delivery audit.
        await db.Notifications
            .Where(x => x.RecipientId == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Subject, _ => "[anonymized]")
                .SetProperty(x => x.Body, _ => "[anonymized]")
                .SetProperty(x => x.DocumentReferencesJson, _ => "[]")
                .SetProperty(x => x.MetadataJson, _ => "{}"),
                cancellationToken);
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private sealed record UserAnonymizedMessage(Guid EventId, DateTimeOffset OccurredAt, Guid UserId, Guid TenantId, string OriginalEmail);
}
