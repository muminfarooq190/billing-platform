using IdentityService.Domain.Common;

namespace IdentityService.Domain.Aggregates;

public sealed class SecurityEvent : AggregateRoot
{
    private SecurityEvent() { }

    private SecurityEvent(Guid id, Guid tenantId, Guid? userId, string eventType, string? ipAddress, string? userAgent, string? metadataJson)
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        EventType = eventType;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        MetadataJson = metadataJson;
        OccurredAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid? UserId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    public static SecurityEvent Create(Guid tenantId, Guid? userId, string eventType, string? ipAddress, string? userAgent, string? metadataJson)
        => new(Guid.NewGuid(), tenantId, userId, eventType, ipAddress, userAgent, metadataJson);
}
