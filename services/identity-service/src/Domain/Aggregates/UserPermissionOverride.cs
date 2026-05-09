using IdentityService.Domain.Common;

namespace IdentityService.Domain.Aggregates;

/// <summary>
/// Per-user permission override applied on top of role-based permissions.
/// `Granted = true` adds the permission, `Granted = false` revokes it
/// even if the user's role grants it.
/// </summary>
public sealed class UserPermissionOverride : AggregateRoot
{
    private UserPermissionOverride() { }

    private UserPermissionOverride(Guid id, Guid userId, Guid tenantId, string permissionKey, bool granted, string? reason)
    {
        Id = id;
        UserId = userId;
        TenantId = tenantId;
        PermissionKey = permissionKey.Trim();
        Granted = granted;
        Reason = reason?.Trim();
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TenantId { get; private set; }
    public string PermissionKey { get; private set; } = string.Empty;
    public bool Granted { get; private set; }
    public string? Reason { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static UserPermissionOverride Create(Guid userId, Guid tenantId, string permissionKey, bool granted, string? reason)
    {
        if (userId == Guid.Empty) throw new InvalidOperationException("UserId is required.");
        if (tenantId == Guid.Empty) throw new InvalidOperationException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(permissionKey)) throw new InvalidOperationException("PermissionKey is required.");
        return new UserPermissionOverride(Guid.NewGuid(), userId, tenantId, permissionKey, granted, reason);
    }

    public void UpdateGrant(bool granted, string? reason)
    {
        Granted = granted;
        Reason = reason?.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
