namespace BillingService.Application.ReadModels;

public sealed class UserFeatureAccessReadModel
{
    public Guid TenantId { get; init; }
    public Guid UserId { get; init; }
    public string FeatureKey { get; init; } = string.Empty;
    public bool TenantGranted { get; init; }
    public bool UserAssigned { get; init; }
    public bool AssignmentRequired { get; init; }
    public bool Granted { get; init; }
    public int? LimitValue { get; init; }
    public string AssignmentMode { get; init; } = string.Empty;
}
