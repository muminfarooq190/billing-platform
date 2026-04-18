using CommunicationService.Application.Commands.SendNotification;
using MediatR;

namespace CommunicationService.Application.Commands.SendWorkflowNotification;

public sealed class SendWorkflowNotificationCommandHandler(IMediator mediator) : IRequestHandler<SendWorkflowNotificationCommand, Guid>
{
    private static readonly Dictionary<string, (string Subject, string Body, string Priority)> Defaults = new(StringComparer.OrdinalIgnoreCase)
    {
        ["quotation-sent"] = ("Your quotation is ready", "Your quotation has been prepared and is ready for review.", "High"),
        ["booking-confirmed"] = ("Your booking is confirmed", "Your booking is confirmed. We'll follow up with the next details shortly.", "High"),
        ["invoice-issued"] = ("Your invoice is ready", "We've issued your invoice. Please review the linked documents for payment details.", "Normal"),
        ["payment-reminder"] = ("Payment reminder", "This is a reminder that payment is still due. Please review the referenced invoice.", "High"),
        ["payment-receipt"] = ("Payment received", "We've received your payment. Your receipt and related documents are available.", "Normal")
    };

    public Task<Guid> Handle(SendWorkflowNotificationCommand request, CancellationToken cancellationToken)
    {
        if (!Defaults.TryGetValue(request.WorkflowType, out var defaults))
            throw new ArgumentOutOfRangeException(nameof(request.WorkflowType), $"Unsupported workflow type '{request.WorkflowType}'.");

        return mediator.Send(new SendNotificationCommand(
            request.TenantId,
            request.RecipientId,
            request.RecipientType,
            request.Channel,
            request.TemplateName,
            string.IsNullOrWhiteSpace(request.Subject) ? defaults.Subject : request.Subject,
            string.IsNullOrWhiteSpace(request.Body) ? defaults.Body : request.Body,
            string.IsNullOrWhiteSpace(request.Priority) ? defaults.Priority : request.Priority!,
            request.ReferenceId,
            request.CorrelationId,
            request.IdempotencyKey,
            request.WorkflowType,
            request.DocumentReferencesJson,
            request.MetadataJson,
            request.Placeholders), cancellationToken);
    }
}
