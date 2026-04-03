using BillingService.Application.ReadModels;
using MediatR;

namespace BillingService.Application.Queries.GetSubscriptionByTenant;

public sealed record GetSubscriptionByTenantQuery(Guid TenantId) : IRequest<SubscriptionReadModel?>;
