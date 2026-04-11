namespace BillingService.Api.Contracts;

public sealed record UpdateTenantFeatureOverrideRequest(
    string FeatureKey,
    bool Granted,
    int? LimitValue,
    string Reason,
    string Source,
    string? CreatedBy,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    string? MetadataJson);
