using System.Text;
using System.Text.Json;
using CommunicationService.Application.Commands.SendNotification;
using MediatR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommunicationService.Infrastructure.Identity;

public sealed class PasswordResetRequestedConsumerService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<PasswordResetRequestedConsumerService> logger) : BackgroundService
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
                channel.QueueDeclare("communication.password-reset", durable: true, exclusive: false, autoDelete: false);
                channel.QueueBind("communication.password-reset", "identity.events", "identity.user.password-reset-requested");
                channel.BasicQos(0, 10, false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (_, args) =>
                {
                    try
                    {
                        var payload = Encoding.UTF8.GetString(args.Body.ToArray());
                        using var scope = scopeFactory.CreateScope();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        var model = JsonSerializer.Deserialize<PasswordResetRequestedMessage>(payload, JsonOptions);
                        if (model is null)
                        {
                            channel.BasicAck(args.DeliveryTag, false);
                            return;
                        }

                        var portalBaseUrl = (configuration["PORTAL_BASE_URL"] ?? configuration["NEXT_PUBLIC_PORTAL_URL"] ?? "http://localhost:3000").TrimEnd('/');
                        var resetUrl = $"{portalBaseUrl}/reset-password?email={Uri.EscapeDataString(model.Email)}&token={Uri.EscapeDataString(model.Token)}";
                        var placeholders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["ResetUrl"] = resetUrl,
                            ["ResetToken"] = model.Token,
                            ["UserEmail"] = model.Email,
                            ["SupportEmail"] = "support@voyara.local"
                        };

                        var command = new SendNotificationCommand(
                            model.TenantId,
                            Guid.NewGuid(),
                            "Admin",
                            "Email",
                            null,
                            "Reset your password",
                            $"We received a request to reset your password.\n\nReset your password: {resetUrl}\n\nIf you did not request this, you can ignore this email.",
                            "High",
                            model.UserId.ToString(),
                            model.EventId.ToString(),
                            $"password-reset:{model.EventId}",
                            "identity.password-reset",
                            "[]",
                            "{}",
                            placeholders);

                        mediator.Send(command, stoppingToken).GetAwaiter().GetResult();
                        channel.BasicAck(args.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to process password reset request event.");
                        channel.BasicNack(args.DeliveryTag, false, requeue: true);
                    }
                };

                channel.BasicConsume("communication.password-reset", autoAck: false, consumer: consumer);

                while (!stoppingToken.IsCancellationRequested && connection.IsOpen)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Password reset consumer crashed; retrying.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private sealed record PasswordResetRequestedMessage(Guid EventId, DateTimeOffset OccurredAt, Guid UserId, Guid TenantId, string Email, string Token);
}
