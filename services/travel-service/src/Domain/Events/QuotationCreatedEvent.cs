using TravelService.Domain.Common;

namespace TravelService.Domain.Events;

public sealed record QuotationCreatedEvent(Guid QuotationId, Guid TenantId, Guid CustomerContactId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
