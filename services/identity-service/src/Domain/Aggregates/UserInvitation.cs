using IdentityService.Domain.Common;
using IdentityService.Domain.Exceptions;

namespace IdentityService.Domain.Aggregates;

public sealed class UserInvitation : AggregateRoot
{
    private UserInvitation() { }

    private UserInvitation(Guid id, Guid tenantId, string email, string role, Guid invitedByUserId, string tokenHash, DateTimeOffset expiresAt)
    {
        Id = id;
        TenantId = tenantId;
        Email = email;
        Role = role;
        InvitedByUserId = invitedByUserId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public Guid InvitedByUserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? AcceptedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static UserInvitation Create(Guid tenantId, string email, string role, Guid invitedByUserId, string tokenHash, DateTimeOffset expiresAt)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Invitation email is required.");
        if (string.IsNullOrWhiteSpace(role)) throw new DomainException("Invitation role is required.");
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new DomainException("Invitation token hash is required.");
        if (expiresAt <= DateTimeOffset.UtcNow) throw new DomainException("Invitation expiry must be in the future.");

        return new UserInvitation(Guid.NewGuid(), tenantId, email.Trim().ToLowerInvariant(), role.Trim(), invitedByUserId, tokenHash, expiresAt);
    }

    public bool IsActive => AcceptedAt is null && RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;

    public void Accept()
    {
        if (RevokedAt.HasValue) throw new DomainException("Invitation has been revoked.");
        if (AcceptedAt.HasValue) throw new DomainException("Invitation has already been accepted.");
        if (ExpiresAt <= DateTimeOffset.UtcNow) throw new DomainException("Invitation has expired.");
        AcceptedAt = DateTimeOffset.UtcNow;
    }

    public void Revoke()
    {
        if (!AcceptedAt.HasValue)
        {
            RevokedAt = DateTimeOffset.UtcNow;
        }
    }

    public void Resend(string tokenHash, DateTimeOffset expiresAt)
    {
        if (AcceptedAt.HasValue) throw new DomainException("Accepted invitation cannot be resent.");
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new DomainException("Invitation token hash is required.");
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        RevokedAt = null;
    }
}
