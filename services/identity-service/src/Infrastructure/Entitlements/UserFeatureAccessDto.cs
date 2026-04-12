namespace IdentityService.Infrastructure.Entitlements;

public sealed record UserFeatureAccessDto(
    Guid TenantId,
    Guid UserId,
    string FeatureKey,
    bool TenantGranted,
    bool UserAssigned,
    bool AssignmentRequired,
    bool Granted,
    int? LimitValue,
    string AssignmentMode);
