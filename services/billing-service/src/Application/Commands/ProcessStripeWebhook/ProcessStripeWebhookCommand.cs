using MediatR;

namespace BillingService.Application.Commands.ProcessStripeWebhook;

public sealed record ProcessStripeWebhookCommand(
    string EventType,
    Guid InvoiceId,
    string? ProviderPaymentId,
    string? ErrorCode,
    string? ErrorMessage,
    /// <summary>Refund id from <c>charge.refunded</c>; null for other events.</summary>
    string? RefundId = null) : IRequest<string>;
