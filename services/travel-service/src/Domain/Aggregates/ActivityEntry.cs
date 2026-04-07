using System.Text.Json;
using TravelService.Domain.Exceptions;

namespace TravelService.Domain.Aggregates;

public sealed class ActivityEntry
{
    private ActivityEntry() { }

    private ActivityEntry(Guid tenantId, string entityType, Guid entityId, string activityType, string summary, string? detailJson, Guid? actorUserId, DateTimeOffset occurredAt)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(entityType))
            throw new DomainException("Entity type is required.");
        if (entityId == Guid.Empty)
            throw new DomainException("Entity id is required.");
        if (string.IsNullOrWhiteSpace(activityType))
            throw new DomainException("Activity type is required.");
        if (string.IsNullOrWhiteSpace(summary))
            throw new DomainException("Summary is required.");

        Id = Guid.NewGuid();
        TenantId = tenantId;
        EntityType = entityType.Trim();
        EntityId = entityId;
        ActivityType = activityType.Trim();
        Summary = summary.Trim();
        DetailJson = string.IsNullOrWhiteSpace(detailJson) ? null : detailJson.Trim();
        ActorUserId = actorUserId;
        OccurredAt = occurredAt;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string ActivityType { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string? DetailJson { get; private set; }
    public Guid? ActorUserId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static ActivityEntry Create(Guid tenantId, string entityType, Guid entityId, string activityType, string summary, object? detail = null, Guid? actorUserId = null, DateTimeOffset? occurredAt = null)
        => new(
            tenantId,
            entityType,
            entityId,
            activityType,
            summary,
            detail is null ? null : JsonSerializer.Serialize(detail),
            actorUserId,
            occurredAt ?? DateTimeOffset.UtcNow);
}
