using IdentityService.Domain.Common;

namespace IdentityService.Domain.Events;

/// <summary>
/// Fired when a user is anonymized for GDPR / right-to-erasure compliance.
/// Consumers (travel-service, communication-service) strip PII keyed off
/// this user id while leaving foreign-key rows intact for audit / legal
/// retention.
///
/// <c>OriginalEmail</c> is included so consumers can match historical
/// rows that were keyed on email rather than user id (e.g. anonymous
/// inquiry rows captured before the user existed).
/// </summary>
public sealed record UserAnonymizedEvent(Guid UserId, Guid TenantId, string OriginalEmail) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
