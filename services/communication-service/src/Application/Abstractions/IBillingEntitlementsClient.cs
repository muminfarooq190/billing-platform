namespace CommunicationService.Application.Abstractions;

public interface IBillingEntitlementsClient
{
    Task<IReadOnlyList<FeatureEntitlementDto>> GetEffectiveEntitlementsAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserFeatureAccessDto>> GetUserFeatureAccessAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken);
}

public sealed record FeatureEntitlementDto(
    string FeatureKey,
    bool Granted,
    string Source,
    string? PlanType,
    int? LimitValue,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    string? MetadataJson);

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
