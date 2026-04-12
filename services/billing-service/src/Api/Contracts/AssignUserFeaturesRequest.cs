namespace BillingService.Api.Contracts;

public sealed class AssignUserFeaturesRequest
{
    public IReadOnlyList<string> FeatureKeys { get; init; } = Array.Empty<string>();
    public Guid? AssignedByUserId { get; init; }
    public DateTimeOffset? EffectiveFrom { get; init; }
    public DateTimeOffset? EffectiveTo { get; init; }
    public string? Notes { get; init; }
    public string? MetadataJson { get; init; }
}
