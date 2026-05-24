using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TravelService.Domain.Aggregates;
using TravelService.Infrastructure.Persistence;

namespace TravelService.Infrastructure.Communication;

/// <summary>
/// Consumes <c>communication.inbound.received</c> from comm-service. Threads
/// inbound SMS/WhatsApp replies back into the originating
/// inquiry/booking by matching the sender's phone number to a Contact
/// row, then appending an internal-visibility note to:
///   - most recent active booking (PrimaryContactId match), else
///   - most recent inquiry where converted_contact_id matches, else
///   - just log + drop (no matching context).
///
/// The note carries `[Inbound SMS] <body>` so it stands out in the
/// timeline. ProviderMessageId stored as metadata so a future inbound
/// thread view can collapse repeats.
///
/// Single-tenant assumption (early-stage Voyara): match across the only
/// tenant in the travel DB. When multi-tenant rolls out, the inbound
/// event will need a TenantId — comm-service should resolve it via the
/// `To` number → tenant lookup.
/// </summary>
public sealed class InboundMessageConsumerService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<InboundMessageConsumerService> logger) : BackgroundService
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
                channel.ExchangeDeclare("communication.events", ExchangeType.Topic, durable: true);
                channel.QueueDeclare("travel.inbound-message", durable: true, exclusive: false, autoDelete: false);
                channel.QueueBind("travel.inbound-message", "communication.events", "communication.inbound.received");
                channel.BasicQos(0, 10, false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (_, args) =>
                {
                    try
                    {
                        var raw = Encoding.UTF8.GetString(args.Body.ToArray());
                        var model = JsonSerializer.Deserialize<InboundMessage>(raw, JsonOptions);
                        if (model is null || string.IsNullOrWhiteSpace(model.From) || string.IsNullOrWhiteSpace(model.Body))
                        {
                            channel.BasicAck(args.DeliveryTag, false);
                            return;
                        }
                        using var scope = scopeFactory.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<TravelDbContext>();
                        ThreadInboundAsync(db, model, stoppingToken).GetAwaiter().GetResult();
                        channel.BasicAck(args.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to thread inbound message.");
                        channel.BasicNack(args.DeliveryTag, false, requeue: true);
                    }
                };

                channel.BasicConsume("travel.inbound-message", autoAck: false, consumer: consumer);

                while (!stoppingToken.IsCancellationRequested && connection.IsOpen)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Inbound message consumer crashed; retrying.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ThreadInboundAsync(TravelDbContext db, InboundMessage msg, CancellationToken cancellationToken)
    {
        // Look up the contact by phone. If multiple contacts share a number
        // (rare — but happens when a household reuses a single mobile), pick
        // the most-recently-updated to attribute the reply to.
        var contact = await db.Contacts
            .Where(c => c.Phone == msg.From && c.DeletedAt == null)
            .OrderByDescending(c => c.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (contact is null)
        {
            logger.LogInformation("Inbound {Channel} from {From} matched no contact — dropping.", msg.Channel, msg.From);
            return;
        }

        var noteBody = $"[Inbound {msg.Channel}] {msg.Body}";

        // Prefer the most recent active booking, fall back to most recent inquiry.
        var booking = await db.Bookings
            .Where(b => b.TenantId == contact.TenantId
                        && b.PrimaryContactId == contact.Id
                        && b.DeletedAt == null
                        && b.CancelledAt == null)
            .OrderByDescending(b => b.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (booking is not null)
        {
            db.EntityNotes.Add(EntityNote.Create(contact.TenantId, "Booking", booking.Id, "Internal", noteBody, null));
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Threaded inbound {Channel} from {From} → booking {BookingId}.", msg.Channel, msg.From, booking.Id);
            return;
        }

        var inquiry = await db.TravelInquiries
            .Where(i => i.TenantId == contact.TenantId
                        && i.ConvertedContactId == contact.Id
                        && i.DeletedAt == null)
            .OrderByDescending(i => i.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (inquiry is not null)
        {
            db.EntityNotes.Add(EntityNote.Create(contact.TenantId, "Inquiry", inquiry.Id, "Internal", noteBody, null));
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Threaded inbound {Channel} from {From} → inquiry {InquiryId}.", msg.Channel, msg.From, inquiry.Id);
            return;
        }

        // Last resort: stash on the contact itself.
        db.EntityNotes.Add(EntityNote.Create(contact.TenantId, "Contact", contact.Id, "Internal", noteBody, null));
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Threaded inbound {Channel} from {From} → contact {ContactId} (no active inquiry/booking).", msg.Channel, msg.From, contact.Id);
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private sealed record InboundMessage(string From, string To, string Body, string Channel, string ProviderMessageId, DateTimeOffset OccurredAt);
}
