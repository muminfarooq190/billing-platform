using BillingService.Application.ReadModels;
using MediatR;

namespace BillingService.Application.Queries.GetBillingDashboard;

public sealed record GetBillingDashboardQuery(Guid TenantId) : IRequest<BillingDashboardReadModel>;
