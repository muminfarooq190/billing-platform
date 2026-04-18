using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommunicationService.Infrastructure.Billing;

public sealed class BillingEventRelayService(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<BillingEventRelayService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { Uri = new Uri(configuration["RABBITMQ_URL"] ?? "amqp://guest:guest@rabbitmq:5672") };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.ExchangeDeclare("billing.events", ExchangeType.Topic, durable: true);
        var queue = channel.QueueDeclare(queue: string.Empty, durable: false, exclusive: true, autoDelete: true).QueueName;
        channel.QueueBind(queue, "billing.events", "billing.invoice.created");
        channel.QueueBind(queue, "billing.events", "billing.invoice.paid");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (_, args) =>
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var clientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
                var client = clientFactory.CreateClient("BillingEventRelayCommunication");
                var payload = Encoding.UTF8.GetString(args.Body.ToArray());
                using var document = JsonDocument.Parse(payload);
                var root = document.RootElement;

                var tenantId = root.TryGetProperty("TenantId", out var tenantIdElement)
                    ? tenantIdElement.GetGuid()
                    : root.GetProperty("tenantId").GetGuid();
                var invoiceId = root.TryGetProperty("InvoiceId", out var invoiceIdElement)
                    ? invoiceIdElement.GetGuid()
                    : root.GetProperty("invoiceId").GetGuid();

                var workflowType = args.RoutingKey.EndsWith("created", StringComparison.OrdinalIgnoreCase)
                    ? "invoice-issued"
                    : "payment-receipt";
                var subject = workflowType == "invoice-issued" ? "Your invoice is ready" : "Payment received";
                var body = workflowType == "invoice-issued"
                    ? "We've issued your invoice. Please review the referenced invoice and complete payment."
                    : "We've received your payment. Your invoice has been marked paid.";

                var request = new
                {
                    recipientId = tenantId,
                    recipientType = "Tenant",
                    channel = "Email",
                    subject,
                    body,
                    priority = workflowType == "invoice-issued" ? "Normal" : "High",
                    referenceId = invoiceId.ToString(),
                    correlationId = invoiceId.ToString(),
                    idempotencyKey = $"{workflowType}:{invoiceId:D}",
                    documents = new[]
                    {
                        new { name = "invoice", documentId = invoiceId.ToString(), url = (string?)null, contentType = "application/json", sizeBytes = (long?)null, metadata = new Dictionary<string, string> { ["source"] = "billing.events" } }
                    },
                    metadata = new Dictionary<string, string> { ["eventType"] = args.RoutingKey }
                };

                using var message = new HttpRequestMessage(HttpMethod.Post, $"communication/notifications/workflows/{workflowType}")
                {
                    Content = JsonContent.Create(request)
                };
                message.Headers.Add("x-tenant-id", tenantId.ToString());
                client.Send(message);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to relay billing event {RoutingKey} into communication workflows", args.RoutingKey);
            }
        };

        channel.BasicConsume(queue, autoAck: true, consumer);

        while (!stoppingToken.IsCancellationRequested)
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
    }
}
