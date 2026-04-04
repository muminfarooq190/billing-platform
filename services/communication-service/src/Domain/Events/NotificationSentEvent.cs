using CommunicationService.Domain.Common;

namespace CommunicationService.Domain.Events;

public sealed record NotificationSentEvent(Guid NotificationId, Guid TenantId, string Channel) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
