using IdentityService.Domain.Common;

namespace IdentityService.Domain.Aggregates;

public sealed class IdentityAuditLog : AggregateRoot
{
    private IdentityAuditLog() { }

    private IdentityAuditLog(Guid id, Guid tenantId, Guid? actorUserId, Guid? targetUserId, string eventType, string? beforeJson, string? afterJson, string? ipAddress, string? userAgent)
    {
        Id = id;
        TenantId = tenantId;
        ActorUserId = actorUserId;
        TargetUserId = targetUserId;
        EventType = eventType;
        BeforeJson = beforeJson;
        AfterJson = afterJson;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        OccurredAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid? ActorUserId { get; private set; }
    public Guid? TargetUserId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string? BeforeJson { get; private set; }
    public string? AfterJson { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    public static IdentityAuditLog Create(Guid tenantId, Guid? actorUserId, Guid? targetUserId, string eventType, string? beforeJson, string? afterJson, string? ipAddress, string? userAgent)
        => new(Guid.NewGuid(), tenantId, actorUserId, targetUserId, eventType, beforeJson, afterJson, ipAddress, userAgent);
}
