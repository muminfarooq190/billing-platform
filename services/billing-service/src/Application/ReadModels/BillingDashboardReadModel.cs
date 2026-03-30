namespace BillingService.Application.ReadModels;

public sealed record BillingDashboardReadModel(Guid TenantId, decimal Mrr, decimal Outstanding, int OverdueCount);
