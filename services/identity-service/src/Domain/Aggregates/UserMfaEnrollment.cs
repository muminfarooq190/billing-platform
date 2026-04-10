using IdentityService.Domain.Common;
using IdentityService.Domain.Exceptions;

namespace IdentityService.Domain.Aggregates;

public sealed class UserMfaEnrollment : AggregateRoot
{
    private UserMfaEnrollment() { }

    private UserMfaEnrollment(Guid id, Guid tenantId, Guid userId, string secret, string recoveryCodesJson)
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        Secret = secret;
        RecoveryCodesJson = recoveryCodesJson;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string Secret { get; private set; } = string.Empty;
    public string RecoveryCodesJson { get; private set; } = "[]";
    public DateTimeOffset? VerifiedAt { get; private set; }
    public DateTimeOffset? DisabledAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public bool IsEnabled => VerifiedAt.HasValue && DisabledAt is null;

    public static UserMfaEnrollment Create(Guid tenantId, Guid userId, string secret, string recoveryCodesJson)
    {
        if (string.IsNullOrWhiteSpace(secret)) throw new DomainException("MFA secret is required.");
        return new UserMfaEnrollment(Guid.NewGuid(), tenantId, userId, secret, string.IsNullOrWhiteSpace(recoveryCodesJson) ? "[]" : recoveryCodesJson);
    }

    public void Verify()
    {
        VerifiedAt = DateTimeOffset.UtcNow;
        DisabledAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Disable()
    {
        DisabledAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
