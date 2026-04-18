using MediatR;

namespace BillingService.Application.Commands.ProcessStripeWebhook;

public sealed record ProcessStripeWebhookCommand(
    string EventType,
    Guid InvoiceId,
    string? ProviderPaymentId,
    string? ErrorCode,
    string? ErrorMessage) : IRequest<string>;
