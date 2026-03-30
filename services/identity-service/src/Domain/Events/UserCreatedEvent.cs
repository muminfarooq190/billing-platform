using IdentityService.Domain.Common;

namespace IdentityService.Domain.Events;

public sealed record UserCreatedEvent(Guid UserId, Guid TenantId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
