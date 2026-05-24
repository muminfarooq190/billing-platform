using MediatR;

namespace IdentityService.Application.Commands.AnonymizeUser;

/// <summary>
/// GDPR right-to-erasure command. Anonymizes the user row in identity-service
/// and emits <c>UserAnonymizedEvent</c> via the outbox so travel-service +
/// communication-service can strip downstream PII keyed on this user id.
///
/// Distinct from <c>DeleteUserCommand</c> which only soft-deletes (sets
/// DeletedAt, leaves email + other PII intact for audit). Use Anonymize
/// for compliance / legal-request flows, SoftDelete for operational
/// account cleanup.
/// </summary>
public sealed record AnonymizeUserCommand(Guid UserId) : IRequest;
