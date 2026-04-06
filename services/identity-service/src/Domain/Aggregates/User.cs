using IdentityService.Domain.Common;
using IdentityService.Domain.Enums;
using IdentityService.Domain.Events;
using IdentityService.Domain.Exceptions;
using IdentityService.Domain.ValueObjects;

namespace IdentityService.Domain.Aggregates;

public sealed class User : AggregateRoot
{
    private User() { }

    private User(Guid id, TenantId tenantId, Email email, string passwordHash, UserRole role)
    {
        Id = id;
        TenantId = tenantId.Value;
        Email = email.Value;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new UserCreatedEvent(Id, TenantId));
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static User Create(TenantId tenantId, Email email, string passwordHash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Password hash is required.");
        }

        return new User(Guid.NewGuid(), tenantId, email, passwordHash, role);
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            throw new DomainException("Password hash is required.");
        }

        PasswordHash = newPasswordHash;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new UserPasswordChangedEvent(Id, TenantId));
    }

    public void UpdateRole(UserRole newRole)
    {
        Role = newRole;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
