namespace IdentityService.Infrastructure.Entitlements;

public sealed class FeatureEntitlementDto
{
    public string FeatureKey { get; set; } = string.Empty;
    public bool Granted { get; set; }
    public string? Source { get; set; }
    public string? PlanType { get; set; }
    public int? LimitValue { get; set; }
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public string? MetadataJson { get; set; }
}
