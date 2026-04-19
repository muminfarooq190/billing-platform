namespace GeoLeadsService.Application.Abstractions;

public interface IBillingEntitlementsClient
{
    Task<IReadOnlyList<FeatureEntitlementDto>> GetEffectiveEntitlementsAsync(Guid tenantId, CancellationToken cancellationToken);
}

public sealed record FeatureEntitlementDto(
    string FeatureKey,
    bool Granted,
    string? Source,
    string? PlanCode,
    int? LimitValue,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    string? MetadataJson);
