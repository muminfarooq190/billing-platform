namespace BillingService.Api.Contracts;

public sealed record GrantFeatureEntitlementRequest(
    string FeatureKey,
    bool Granted,
    int? LimitValue,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    string? Reason);
