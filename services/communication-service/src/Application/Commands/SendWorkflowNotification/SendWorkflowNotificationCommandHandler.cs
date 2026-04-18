using System.Text;
using System.Text.Json;
using CommunicationService.Application.Commands.SendNotification;
using MediatR;

namespace CommunicationService.Application.Commands.SendWorkflowNotification;

public sealed class SendWorkflowNotificationCommandHandler(IMediator mediator) : IRequestHandler<SendWorkflowNotificationCommand, Guid>
{
    private static readonly Dictionary<string, (string Subject, string Body, string Priority)> Defaults = new(StringComparer.OrdinalIgnoreCase)
    {
        ["quotation-sent"] = ("Your quotation is ready", "Your quotation has been prepared and is ready for review.", "High"),
        ["itinerary-sent"] = ("Your itinerary is ready", "Your itinerary is ready for review.", "High"),
        ["booking-confirmed"] = ("Your booking is confirmed", "Your booking is confirmed. We'll follow up with the next details shortly.", "High"),
        ["invoice-issued"] = ("Your invoice is ready", "We've issued your invoice. Please review the linked documents for payment details.", "Normal"),
        ["payment-reminder"] = ("Payment reminder", "This is a reminder that payment is still due. Please review the referenced invoice.", "High"),
        ["payment-receipt"] = ("Payment received", "We've received your payment. Your receipt and related documents are available.", "Normal")
    };

    public Task<Guid> Handle(SendWorkflowNotificationCommand request, CancellationToken cancellationToken)
    {
        if (!Defaults.TryGetValue(request.WorkflowType, out var defaults))
            throw new ArgumentOutOfRangeException(nameof(request.WorkflowType), $"Unsupported workflow type '{request.WorkflowType}'.");

        var body = string.IsNullOrWhiteSpace(request.Body) ? defaults.Body : request.Body!;
        if (!string.IsNullOrWhiteSpace(request.DocumentReferencesJson) && request.DocumentReferencesJson != "[]")
            body = AppendDocuments(body, request.DocumentReferencesJson);

        return mediator.Send(new SendNotificationCommand(
            request.TenantId,
            request.RecipientId,
            request.RecipientType,
            request.Channel,
            request.TemplateName,
            string.IsNullOrWhiteSpace(request.Subject) ? defaults.Subject : request.Subject,
            body,
            string.IsNullOrWhiteSpace(request.Priority) ? defaults.Priority : request.Priority!,
            request.ReferenceId,
            request.CorrelationId,
            request.IdempotencyKey,
            request.WorkflowType,
            request.DocumentReferencesJson,
            request.MetadataJson,
            request.Placeholders), cancellationToken);
    }

    private static string AppendDocuments(string body, string documentsJson)
    {
        try
        {
            var docs = JsonSerializer.Deserialize<List<DocumentPayload>>(documentsJson) ?? [];
            if (docs.Count == 0)
                return body;

            var sb = new StringBuilder(body);
            sb.Append("\n\n[DocumentReferencesJson]\n");
            sb.Append(documentsJson);
            return sb.ToString();
        }
        catch (JsonException)
        {
            return body;
        }
    }

    private sealed record DocumentPayload(string Name, string? DocumentId, string? Url, string? ContentType, long? SizeBytes, Dictionary<string, string>? Metadata);
}
