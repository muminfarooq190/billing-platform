using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TravelService.Infrastructure.Persistence;

namespace TravelService.Infrastructure.Identity;

/// <summary>
/// Consumes <c>identity.user.anonymized</c> from the identity-service topic
/// exchange. On receipt, clears the user's assignment trail from
/// travel-service rows so the anonymized user no longer surfaces in
/// pickers / activity feeds.
///
/// GDPR scope here is **the agent/employee's personal data trail**, NOT the
/// customer/contact PII (which belongs to the tenant's customer, not the
/// agent). So we:
///   - Null out <c>assigned_to_user_id</c> on inquiries / quotations /
///     bookings / follow-ups owned by this user (unassignment cascade).
///   - Leave contact/traveler PII intact — those rows belong to the
///     customer/traveller, not the agent.
///
/// Idempotent: re-processing the same event sets already-null fields to
/// null again — no-op.
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
                channel.QueueDeclare("travel.user-anonymized", durable: true, exclusive: false, autoDelete: false);
                channel.QueueBind("travel.user-anonymized", "identity.events", "identity.user.anonymized");
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
                        var db = scope.ServiceProvider.GetRequiredService<TravelDbContext>();
                        ClearAssignmentsAsync(db, model.UserId, stoppingToken).GetAwaiter().GetResult();

                        channel.BasicAck(args.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to process UserAnonymized event.");
                        channel.BasicNack(args.DeliveryTag, false, requeue: true);
                    }
                };

                channel.BasicConsume("travel.user-anonymized", autoAck: false, consumer: consumer);

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

    /// <summary>
    /// Bulk-clear assigned_to_user_id across travel-service tables. Uses
    /// EF Core 8 ExecuteUpdate so no in-memory tracking is needed for
    /// what could be thousands of rows.
    /// </summary>
    private static async Task ClearAssignmentsAsync(TravelDbContext db, Guid userId, CancellationToken cancellationToken)
    {
        await db.TravelInquiries
            .Where(x => x.AssignedToUserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.AssignedToUserId, _ => null), cancellationToken);
        // Quotation aggregate has no AssignedToUserId field — assignment
        // flows through the parent inquiry/booking. Skip.
        await db.Bookings
            .Where(x => x.AssignedToUserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.AssignedToUserId, _ => null), cancellationToken);
        await db.FollowUps
            .Where(x => x.AssignedToUserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.AssignedToUserId, _ => null), cancellationToken);
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private sealed record UserAnonymizedMessage(Guid EventId, DateTimeOffset OccurredAt, Guid UserId, Guid TenantId, string OriginalEmail);
}
