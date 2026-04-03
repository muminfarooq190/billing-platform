using BillingService.Domain.Common;
using BillingService.Domain.Enums;

namespace BillingService.Domain.Events;

public sealed record PaymentProcessedEvent(Guid InvoiceId, Guid TenantId, PaymentResult Result) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
