using BillingService.Application.ReadModels;
using BillingService.Domain.Enums;
using BillingService.Domain.Repositories;
using MediatR;

namespace BillingService.Application.Queries.GetTenantFeatureAllocations;

public sealed class GetTenantFeatureAllocationsQueryHandler(
    IFeatureCatalogRepository featureCatalogRepository,
    ITenantUserFeatureAssignmentRepository assignmentRepository,
    IMediator mediator) : IRequestHandler<GetTenantFeatureAllocationsQuery, IReadOnlyList<TenantFeatureAllocationReadModel>>
{
    public async Task<IReadOnlyList<TenantFeatureAllocationReadModel>> Handle(GetTenantFeatureAllocationsQuery request, CancellationToken cancellationToken)
    {
        var entitlements = await mediator.Send(new GetEffectiveEntitlements.GetEffectiveEntitlementsQuery(request.TenantId), cancellationToken);
        var grantedEntitlements = entitlements.Where(x => x.Granted).ToList();
        var catalog = await featureCatalogRepository.ListAsync(cancellationToken);
        var catalogByKey = catalog.ToDictionary(x => x.FeatureKey, StringComparer.OrdinalIgnoreCase);

        var results = new List<TenantFeatureAllocationReadModel>();
        foreach (var entitlement in grantedEntitlements)
        {
            catalogByKey.TryGetValue(entitlement.FeatureKey, out var feature);
            var mode = feature?.AssignmentMode ?? FeatureAssignmentMode.TenantWide;
            var maxAssignments = feature?.DefaultAssignmentLimit;
            var activeAssignments = mode == FeatureAssignmentMode.TenantWide
                ? 0
                : await assignmentRepository.CountActiveAssignmentsAsync(request.TenantId, entitlement.FeatureKey, cancellationToken);

            results.Add(new TenantFeatureAllocationReadModel
            {
                FeatureKey = entitlement.FeatureKey,
                TenantGranted = entitlement.Granted,
                AssignmentMode = mode.ToString(),
                AssignmentRequired = mode != FeatureAssignmentMode.TenantWide,
                MaxAssignments = maxAssignments,
                ActiveAssignments = activeAssignments,
                RemainingAssignments = maxAssignments.HasValue ? Math.Max(maxAssignments.Value - activeAssignments, 0) : null,
                LimitValue = entitlement.LimitValue
            });
        }

        return results.OrderBy(x => x.FeatureKey).ToList();
    }
}
