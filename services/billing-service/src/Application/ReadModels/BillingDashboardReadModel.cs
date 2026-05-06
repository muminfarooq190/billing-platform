namespace BillingService.Application.ReadModels;

public sealed class BillingDashboardReadModel
{
    public Guid TenantId { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal OutstandingAmount { get; init; }
    public decimal OverdueAmount { get; init; }
    public int PaidInvoicesCount { get; init; }
    public int UnpaidInvoicesCount { get; init; }
    public string? Currency { get; init; }
}
