using IdentityService.Domain.Common;

namespace IdentityService.Domain.Aggregates;

public sealed class RolePermissionAssignment : AggregateRoot
{
    private RolePermissionAssignment() { }

    private RolePermissionAssignment(Guid id, Guid roleDefinitionId, string permissionKey)
    {
        Id = id;
        RoleDefinitionId = roleDefinitionId;
        PermissionKey = permissionKey;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid RoleDefinitionId { get; private set; }
    public string PermissionKey { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    public static RolePermissionAssignment Create(Guid roleDefinitionId, string permissionKey)
        => new(Guid.NewGuid(), roleDefinitionId, permissionKey.Trim());
}
