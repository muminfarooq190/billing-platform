using System.Text.Json;
using TravelService.Domain.Exceptions;

namespace TravelService.Domain.Aggregates;

public sealed class AuditLog
{
    private AuditLog() { }

    private AuditLog(Guid tenantId, string entityType, Guid entityId, string action, Guid? actorUserId, string? ipAddress, string? userAgent, string? beforeJson, string? afterJson, string? metadataJson, DateTimeOffset occurredAt)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(entityType))
            throw new DomainException("Entity type is required.");
        if (entityId == Guid.Empty)
            throw new DomainException("Entity id is required.");
        if (string.IsNullOrWhiteSpace(action))
            throw new DomainException("Action is required.");

        Id = Guid.NewGuid();
        TenantId = tenantId;
        EntityType = entityType.Trim();
        EntityId = entityId;
        Action = action.Trim();
        ActorUserId = actorUserId;
        IpAddress = NormalizeOptional(ipAddress);
        UserAgent = NormalizeOptional(userAgent);
        BeforeJson = NormalizeOptional(beforeJson);
        AfterJson = NormalizeOptional(afterJson);
        MetadataJson = NormalizeOptional(metadataJson);
        OccurredAt = occurredAt;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public Guid? ActorUserId { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? BeforeJson { get; private set; }
    public string? AfterJson { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    public static AuditLog Create(Guid tenantId, string entityType, Guid entityId, string action, Guid? actorUserId = null, string? ipAddress = null, string? userAgent = null, object? before = null, object? after = null, object? metadata = null, DateTimeOffset? occurredAt = null)
        => new(
            tenantId,
            entityType,
            entityId,
            action,
            actorUserId,
            ipAddress,
            userAgent,
            before is null ? null : JsonSerializer.Serialize(before),
            after is null ? null : JsonSerializer.Serialize(after),
            metadata is null ? null : JsonSerializer.Serialize(metadata),
            occurredAt ?? DateTimeOffset.UtcNow);

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
