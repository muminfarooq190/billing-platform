using CommunicationService.Domain.Common;

namespace CommunicationService.Domain.Events;

public sealed record NotificationFailedEvent(Guid NotificationId, Guid TenantId, string Channel, string Reason) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
