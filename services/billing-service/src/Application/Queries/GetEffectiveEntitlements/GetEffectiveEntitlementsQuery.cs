using BillingService.Application.ReadModels;
using MediatR;

namespace BillingService.Application.Queries.GetEffectiveEntitlements;

public sealed record GetEffectiveEntitlementsQuery(Guid TenantId) : IRequest<IReadOnlyList<FeatureEntitlementReadModel>>;
