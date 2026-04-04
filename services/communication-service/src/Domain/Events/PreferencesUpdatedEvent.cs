using CommunicationService.Domain.Common;

namespace CommunicationService.Domain.Events;

public sealed record PreferencesUpdatedEvent(Guid PreferencesId, Guid TenantId, Guid RecipientId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
