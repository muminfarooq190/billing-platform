using BillingService.Domain.Common;

namespace BillingService.Domain.Events;

public sealed record InvoicePaidEvent(
    Guid InvoiceId,
    Guid TenantId,
    Guid SubscriptionId,
    string Status,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset PaidAt,
    string? PaymentGateway,
    string? ProviderPaymentId,
    string PricingReference) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}