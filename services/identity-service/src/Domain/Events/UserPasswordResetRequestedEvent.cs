using IdentityService.Domain.Common;

namespace IdentityService.Domain.Events;

public sealed record UserPasswordResetRequestedEvent(Guid UserId, Guid TenantId, string Email, string Token) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
