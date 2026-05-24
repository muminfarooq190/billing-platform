using IdentityService.Domain.Common;
using System.Security.Cryptography;
using System.Text;
using IdentityService.Domain.Enums;
using IdentityService.Domain.Events;
using IdentityService.Domain.Exceptions;
using IdentityService.Domain.ValueObjects;

namespace IdentityService.Domain.Aggregates;

public sealed class User : AggregateRoot
{
    private User() { }

    private User(Guid id, TenantId tenantId, Email email, string? passwordHash, UserRole role, UserStatus status, bool mustChangePassword)
    {
        Id = id;
        TenantId = tenantId.Value;
        Email = email.Value;
        PasswordHash = passwordHash ?? string.Empty;
        Role = role;
        Status = status;
        MustChangePassword = mustChangePassword;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new UserCreatedEvent(Id, TenantId));
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }
    public DateTimeOffset? PasswordChangedAt { get; private set; }
    public bool MustChangePassword { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static User Create(TenantId tenantId, Email email, string passwordHash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Password hash is required.");
        }

        return new User(Guid.NewGuid(), tenantId, email, passwordHash, role, UserStatus.Active, false);
    }

    public static User Invite(TenantId tenantId, Email email, UserRole role)
        => new(Guid.NewGuid(), tenantId, email, null, role, UserStatus.Invited, true);

    public void AcceptInvitation(string passwordHash)
    {
        if (Status != UserStatus.Invited)
        {
            throw new DomainException("Only invited users can accept invitations.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Password hash is required.");
        }

        PasswordHash = passwordHash;
        Status = UserStatus.Active;
        MustChangePassword = false;
        PasswordChangedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            throw new DomainException("Password hash is required.");
        }

        PasswordHash = newPasswordHash;
        PasswordChangedAt = DateTimeOffset.UtcNow;
        MustChangePassword = false;
        if (Status == UserStatus.Invited)
        {
            Status = UserStatus.Active;
        }
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new UserPasswordChangedEvent(Id, TenantId));
    }

    public PasswordResetToken RequestPasswordReset(TimeSpan lifetime)
    {
        if (Status != UserStatus.Active)
        {
            throw new DomainException("Only active users can request password reset.");
        }

        if (lifetime <= TimeSpan.Zero)
        {
            throw new DomainException("Password reset lifetime must be greater than zero.");
        }

        var rawToken = Guid.NewGuid().ToString("N");
        var token = PasswordResetToken.Create(Id, Email, Hash(rawToken), DateTimeOffset.UtcNow.Add(lifetime));
        AddDomainEvent(new UserPasswordResetRequestedEvent(Id, TenantId, Email, rawToken));
        UpdatedAt = DateTimeOffset.UtcNow;
        return token;
    }

    public void UpdateRole(UserRole newRole)
    {
        Role = newRole;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkLogin()
    {
        if (Status == UserStatus.Suspended || Status == UserStatus.Locked || Status == UserStatus.Deleted)
        {
            throw new DomainException("User cannot log in with current status.");
        }

        if (Status == UserStatus.Invited)
        {
            throw new DomainException("Invited user must accept invitation first.");
        }

        LastLoginAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Suspend()
    {
        Status = UserStatus.Suspended;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reactivate()
    {
        if (Status == UserStatus.Deleted)
        {
            throw new DomainException("Deleted user cannot be reactivated.");
        }

        Status = UserStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Lock()
    {
        Status = UserStatus.Locked;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTimeOffset.UtcNow;
        Status = UserStatus.Deleted;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// GDPR right-to-erasure. Blanks PII while keeping the row + FKs intact
    /// for audit / legal retention. Emits <c>UserAnonymizedEvent</c> for
    /// cross-service cascade. Idempotent — second call is a no-op.
    /// </summary>
    public void Anonymize()
    {
        if (Status == UserStatus.Deleted) return;
        var originalEmail = Email;
        Email = $"deleted+{Id:N}@anonymized.invalid";
        PasswordHash = string.Empty;
        Status = UserStatus.Deleted;
        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new UserAnonymizedEvent(Id, TenantId, originalEmail));
    }

    private static string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
