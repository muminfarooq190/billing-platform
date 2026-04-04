namespace BillingService.Application.ReadModels;

public sealed class BillingDashboardReadModel
{
    public Guid TenantId { get; init; }
    public decimal Mrr { get; init; }
    public decimal Outstanding { get; init; }
    public int OverdueCount { get; init; }
}
