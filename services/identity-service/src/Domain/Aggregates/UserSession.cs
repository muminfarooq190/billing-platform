using IdentityService.Domain.Common;

namespace IdentityService.Domain.Aggregates;

public sealed class UserSession : AggregateRoot
{
    private UserSession() { }

    private UserSession(Guid id, Guid tenantId, Guid userId, string refreshTokenId, string? deviceName, string? ipAddress, string? userAgent)
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        RefreshTokenId = refreshTokenId;
        DeviceName = deviceName;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CreatedAt = DateTimeOffset.UtcNow;
        LastSeenAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string RefreshTokenId { get; private set; } = string.Empty;
    public string? DeviceName { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public static UserSession Create(Guid tenantId, Guid userId, string refreshTokenId, string? deviceName, string? ipAddress, string? userAgent)
        => new(Guid.NewGuid(), tenantId, userId, refreshTokenId, deviceName, ipAddress, userAgent);

    public void Touch(string? ipAddress, string? userAgent)
    {
        IpAddress = ipAddress ?? IpAddress;
        UserAgent = userAgent ?? UserAgent;
        LastSeenAt = DateTimeOffset.UtcNow;
    }

    public void Revoke()
    {
        RevokedAt ??= DateTimeOffset.UtcNow;
    }
}
