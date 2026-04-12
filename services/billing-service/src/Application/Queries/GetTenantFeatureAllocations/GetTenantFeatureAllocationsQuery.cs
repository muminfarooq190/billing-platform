using BillingService.Application.ReadModels;
using MediatR;

namespace BillingService.Application.Queries.GetTenantFeatureAllocations;

public sealed record GetTenantFeatureAllocationsQuery(Guid TenantId) : IRequest<IReadOnlyList<TenantFeatureAllocationReadModel>>;
