using System.Text;
using System.Text.Json;
using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Enums;
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
                        var dispatchers = scope.ServiceProvider.GetServices<IChannelDispatcher>().ToDictionary(d => d.Channel, d => d);
                        var model = JsonSerializer.Deserialize<PasswordResetRequestedMessage>(payload, JsonOptions);
                        if (model is null)
                        {
                            channel.BasicAck(args.DeliveryTag, false);
                            return;
                        }

                        if (!dispatchers.TryGetValue(ChannelType.Email, out var emailDispatcher))
                        {
                            throw new InvalidOperationException("Email dispatcher is not registered.");
                        }

                        var portalBaseUrl = (configuration["PORTAL_BASE_URL"] ?? configuration["NEXT_PUBLIC_PORTAL_URL"] ?? "http://localhost:3000").TrimEnd('/');
                        var supportEmail = configuration["EMAIL_DEFAULT_FROM_EMAIL"] ?? "support@voyara.local";
                        var resetUrl = $"{portalBaseUrl}/reset-password?email={Uri.EscapeDataString(model.Email)}&token={Uri.EscapeDataString(model.Token)}";
                        var subject = "Reset your password";
                        var body = $"We received a request to reset your password.\n\nReset your password: {resetUrl}\n\nIf you did not request this, you can ignore this email or contact {supportEmail}.";

                        var result = emailDispatcher.SendAsync(model.Email, subject, body, stoppingToken).GetAwaiter().GetResult();
                        if (!result.Success)
                        {
                            throw new InvalidOperationException(result.ErrorMessage ?? "Password reset email dispatch failed.");
                        }

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
