namespace BillingService.Application.ReadModels;

public sealed class FeatureEntitlementReadModel
{
    public string FeatureKey { get; init; } = string.Empty;
    public bool Granted { get; init; }
    public string Source { get; init; } = string.Empty;
    public string? PlanType { get; init; }
    public int? LimitValue { get; init; }
    public DateTimeOffset EffectiveFrom { get; init; }
    public DateTimeOffset? EffectiveTo { get; init; }
    public string? MetadataJson { get; init; }
}
