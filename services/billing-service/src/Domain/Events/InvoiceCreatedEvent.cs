using BillingService.Domain.Common;

namespace BillingService.Domain.Events;

public sealed record InvoiceCreatedEvent(
    Guid InvoiceId,
    Guid TenantId,
    Guid SubscriptionId,
    string Status,
    decimal SubtotalAmount,
    string Currency,
    decimal TaxAmount,
    decimal TotalAmount,
    DateTimeOffset DueDate,
    DateOnly BillingPeriodStart,
    DateOnly BillingPeriodEnd,
    string PricingReference,
    IReadOnlyList<InvoiceCreatedLineItem> LineItems) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record InvoiceCreatedLineItem(
    string Description,
    int Quantity,
    decimal UnitPriceAmount,
    string Currency,
    decimal LineTotalAmount);
