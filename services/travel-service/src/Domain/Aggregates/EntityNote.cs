using TravelService.Domain.Exceptions;

namespace TravelService.Domain.Aggregates;

public sealed class EntityNote
{
    private static readonly HashSet<string> AllowedVisibilities = ["Internal", "CustomerVisible"];

    private EntityNote() { }

    private EntityNote(Guid tenantId, string entityType, Guid entityId, string visibility, string content, Guid? createdByUserId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(entityType))
            throw new DomainException("Entity type is required.");
        if (entityId == Guid.Empty)
            throw new DomainException("Entity id is required.");
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Content is required.");

        Id = Guid.NewGuid();
        TenantId = tenantId;
        EntityType = entityType.Trim();
        EntityId = entityId;
        Visibility = NormalizeVisibility(visibility);
        Content = content.Trim();
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Visibility { get; private set; } = "Internal";
    public string Content { get; private set; } = string.Empty;
    public Guid? CreatedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static EntityNote Create(Guid tenantId, string entityType, Guid entityId, string visibility, string content, Guid? createdByUserId = null)
        => new(tenantId, entityType, entityId, visibility, content, createdByUserId);

    public void Update(string visibility, string content)
    {
        if (DeletedAt is not null)
            throw new DomainException("Deleted notes cannot be updated.");
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Content is required.");

        Visibility = NormalizeVisibility(visibility);
        Content = content.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Delete()
    {
        if (DeletedAt is not null)
            throw new DomainException("Note is already deleted.");

        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizeVisibility(string visibility)
    {
        if (string.IsNullOrWhiteSpace(visibility))
            throw new DomainException("Visibility is required.");

        var normalized = visibility.Trim();
        if (!AllowedVisibilities.Contains(normalized))
            throw new DomainException($"Visibility '{normalized}' is not supported.");

        return normalized;
    }
}
