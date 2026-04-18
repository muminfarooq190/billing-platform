using System.Net.Http.Json;
using TravelService.Application.Abstractions;

namespace TravelService.Infrastructure.Communication;

public sealed class CommunicationWorkflowClient(HttpClient httpClient) : ICommunicationWorkflowClient
{
    public async Task SendQuotationAsync(Guid tenantId, QuotationCommunicationRequest request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "communication/notifications/workflows/quotation-sent")
        {
            Content = JsonContent.Create(new
            {
                recipientId = request.RecipientId,
                recipientType = "Customer",
                channel = request.Channel,
                subject = request.Subject,
                body = request.Body,
                priority = "High",
                referenceId = request.ReferenceId,
                correlationId = request.CorrelationId,
                idempotencyKey = request.IdempotencyKey,
                documents = request.Documents,
                metadata = new Dictionary<string, string> { ["source"] = "travel-service", ["workflow"] = "quotation-sent" }
            })
        };
        message.Headers.Add("x-tenant-id", tenantId.ToString());
        using var response = await httpClient.SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendItineraryAsync(Guid tenantId, ItineraryCommunicationRequest request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "communication/notifications/workflows/itinerary-sent")
        {
            Content = JsonContent.Create(new
            {
                recipientId = request.RecipientId,
                recipientType = "Customer",
                channel = request.Channel,
                subject = request.Subject,
                body = request.Body,
                priority = "High",
                referenceId = request.ReferenceId,
                correlationId = request.CorrelationId,
                idempotencyKey = request.IdempotencyKey,
                documents = request.Documents,
                metadata = new Dictionary<string, string> { ["source"] = "travel-service", ["workflow"] = "itinerary-sent" }
            })
        };
        message.Headers.Add("x-tenant-id", tenantId.ToString());
        using var response = await httpClient.SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
