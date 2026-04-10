using IdentityService.Domain.Common;

namespace IdentityService.Domain.Aggregates;

public sealed class UserRoleAssignment : AggregateRoot
{
    private UserRoleAssignment() { }

    private UserRoleAssignment(Guid id, Guid tenantId, Guid userId, Guid roleDefinitionId)
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        RoleDefinitionId = roleDefinitionId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid RoleDefinitionId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static UserRoleAssignment Create(Guid tenantId, Guid userId, Guid roleDefinitionId)
        => new(Guid.NewGuid(), tenantId, userId, roleDefinitionId);
}
