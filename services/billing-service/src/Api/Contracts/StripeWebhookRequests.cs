namespace BillingService.Api.Contracts;

public sealed record StripeWebhookRequest(
    string EventType,
    string InvoiceId,
    string? ProviderPaymentId,
    string? Signature,
    string? ErrorCode,
    string? ErrorMessage);
