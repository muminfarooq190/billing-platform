using IdentityService.Domain.Common;
using IdentityService.Domain.Exceptions;

namespace IdentityService.Domain.Aggregates;

public sealed class RoleDefinition : AggregateRoot
{
    private readonly List<RolePermissionAssignment> _permissions = [];

    private RoleDefinition() { }

    private RoleDefinition(Guid id, Guid? tenantId, string name, string normalizedName, string description, bool isSystemDefault)
    {
        Id = id;
        TenantId = tenantId;
        Name = name;
        NormalizedName = normalizedName;
        Description = description;
        IsSystemDefault = isSystemDefault;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid? TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsSystemDefault { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public IReadOnlyCollection<RolePermissionAssignment> Permissions => _permissions;

    public static RoleDefinition Create(Guid? tenantId, string name, string description, bool isSystemDefault = false)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Role name is required.");
        var normalized = name.Trim().ToUpperInvariant();
        return new RoleDefinition(Guid.NewGuid(), tenantId, name.Trim(), normalized, description?.Trim() ?? string.Empty, isSystemDefault);
    }

    public void Update(string name, string description)
    {
        if (IsSystemDefault) throw new DomainException("System roles cannot be renamed.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Role name is required.");
        Name = name.Trim();
        NormalizedName = Name.ToUpperInvariant();
        Description = description?.Trim() ?? string.Empty;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetPermissions(IEnumerable<string> permissionKeys)
    {
        _permissions.Clear();
        foreach (var key in permissionKeys.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            _permissions.Add(RolePermissionAssignment.Create(Id, key));
        }
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
