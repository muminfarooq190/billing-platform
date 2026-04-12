using IdentityService.Domain.Common;
using IdentityService.Domain.Enums;
using IdentityService.Domain.Events;
using IdentityService.Domain.Exceptions;
using IdentityService.Domain.ValueObjects;

namespace IdentityService.Domain.Aggregates;

public sealed class Tenant : AggregateRoot
{
    private Tenant() { }

    private Tenant(TenantId id, string name, Email email)
    {
        Id = id.Value;
        Name = name;
        Email = email.Value;
        Status = TenantStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new TenantCreatedEvent(Id));
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public TenantStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static Tenant Register(string name, Email email)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Tenant name is required.");
        }

        return new Tenant(TenantId.New(), name.Trim(), email);
    }

    public void Suspend()
    {
        if (Status == TenantStatus.Deleted)
        {
            throw new DomainException("Deleted tenant cannot be suspended.");
        }

        Status = TenantStatus.Suspended;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new TenantSuspendedEvent(Id));
    }

    public void SoftDelete()
    {
        Status = TenantStatus.Deleted;
        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
