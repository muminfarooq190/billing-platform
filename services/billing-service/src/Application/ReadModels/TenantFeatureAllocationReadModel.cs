namespace BillingService.Application.ReadModels;

public sealed class TenantFeatureAllocationReadModel
{
    public string FeatureKey { get; init; } = string.Empty;
    public bool TenantGranted { get; init; }
    public string AssignmentMode { get; init; } = string.Empty;
    public bool AssignmentRequired { get; init; }
    public int? MaxAssignments { get; init; }
    public int ActiveAssignments { get; init; }
    public int? RemainingAssignments { get; init; }
    public int? LimitValue { get; init; }
}
