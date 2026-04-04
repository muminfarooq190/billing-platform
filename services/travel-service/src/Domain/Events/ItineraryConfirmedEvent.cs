using TravelService.Domain.Common;

namespace TravelService.Domain.Events;

public sealed record ItineraryConfirmedEvent(Guid ItineraryId, Guid TenantId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
