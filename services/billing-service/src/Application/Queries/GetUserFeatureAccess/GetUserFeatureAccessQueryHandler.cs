using BillingService.Application.ReadModels;
using BillingService.Domain.Enums;
using BillingService.Domain.Repositories;
using MediatR;

namespace BillingService.Application.Queries.GetUserFeatureAccess;

public sealed class GetUserFeatureAccessQueryHandler(
    IFeatureCatalogRepository featureCatalogRepository,
    ITenantUserFeatureAssignmentRepository assignmentRepository,
    IMediator mediator) :
    IRequestHandler<GetUserFeatureAccessQuery, IReadOnlyList<UserFeatureAccessReadModel>>,
    IRequestHandler<Queries.GetMyFeatureAccess.GetMyFeatureAccessQuery, IReadOnlyList<UserFeatureAccessReadModel>>
{
    public async Task<IReadOnlyList<UserFeatureAccessReadModel>> Handle(GetUserFeatureAccessQuery request, CancellationToken cancellationToken)
        => await ResolveAsync(request.TenantId, request.UserId, cancellationToken);

    public async Task<IReadOnlyList<UserFeatureAccessReadModel>> Handle(Queries.GetMyFeatureAccess.GetMyFeatureAccessQuery request, CancellationToken cancellationToken)
        => await ResolveAsync(request.TenantId, request.UserId, cancellationToken);

    private async Task<IReadOnlyList<UserFeatureAccessReadModel>> ResolveAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken)
    {
        var entitlements = await mediator.Send(new Queries.GetEffectiveEntitlements.GetEffectiveEntitlementsQuery(tenantId), cancellationToken);
        var assignments = await assignmentRepository.ListByTenantIdAndUserIdAsync(tenantId, userId, cancellationToken);
        var activeAssignments = assignments.Where(x => x.IsEffectiveAt(DateTimeOffset.UtcNow)).ToLookup(x => x.FeatureKey, StringComparer.OrdinalIgnoreCase);
        var catalog = await featureCatalogRepository.ListAsync(cancellationToken);
        var catalogByKey = catalog.ToDictionary(x => x.FeatureKey, StringComparer.OrdinalIgnoreCase);

        return entitlements
            .OrderBy(x => x.FeatureKey)
            .Select(entitlement =>
            {
                catalogByKey.TryGetValue(entitlement.FeatureKey, out var feature);
                var mode = feature?.AssignmentMode ?? FeatureAssignmentMode.TenantWide;
                var assignmentRequired = mode != FeatureAssignmentMode.TenantWide;
                var userAssigned = !assignmentRequired || activeAssignments[entitlement.FeatureKey].Any();

                return new UserFeatureAccessReadModel
                {
                    TenantId = tenantId,
                    UserId = userId,
                    FeatureKey = entitlement.FeatureKey,
                    TenantGranted = entitlement.Granted,
                    UserAssigned = userAssigned,
                    AssignmentRequired = assignmentRequired,
                    Granted = entitlement.Granted && userAssigned,
                    LimitValue = entitlement.LimitValue,
                    AssignmentMode = mode.ToString()
                };
            })
            .ToList();
    }
}
