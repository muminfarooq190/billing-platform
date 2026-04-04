using CommunicationService.Domain.Common;

namespace CommunicationService.Domain.Events;

public sealed record TemplateCreatedEvent(Guid TemplateId, Guid TenantId, string Name) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
