using IdentityService.Domain.Common;

namespace IdentityService.Domain.Aggregates;

public sealed class PermissionDefinition : AggregateRoot
{
    private PermissionDefinition() { }

    private PermissionDefinition(Guid id, string key, string category, string description)
    {
        Id = id;
        Key = key;
        Category = category;
        Description = description;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    public static PermissionDefinition Create(string key, string category, string description)
        => new(Guid.NewGuid(), key.Trim(), category.Trim(), description.Trim());
}
