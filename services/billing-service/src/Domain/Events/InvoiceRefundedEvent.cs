using BillingService.Domain.Common;

namespace BillingService.Domain.Events;

/// <summary>
/// Fired when a paid invoice is fully refunded via the payment gateway
/// (typically Stripe `charge.refunded`). Subscribe in communication-service
/// for receipt-of-refund email; subscribe in travel-service if you need to
/// surface "Refunded" on the booking ledger.
/// </summary>
public sealed record InvoiceRefundedEvent(
    Guid InvoiceId,
    Guid TenantId,
    Guid SubscriptionId,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset RefundedAt,
    string? PaymentGateway,
    string? ProviderRefundId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
