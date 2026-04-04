using TravelService.Domain.Common;

namespace TravelService.Domain.Events;

public sealed record ItineraryCreatedEvent(Guid ItineraryId, Guid TenantId, Guid? QuotationId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
