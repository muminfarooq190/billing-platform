using BillingService.Application.Abstractions;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Enums;
using BillingService.Domain.Exceptions;
using BillingService.Domain.Repositories;
using MediatR;

namespace BillingService.Application.Commands.AssignUserFeatures;

public sealed class AssignUserFeaturesCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IFeatureCatalogRepository featureCatalogRepository,
    ITenantUserFeatureAssignmentRepository assignmentRepository,
    IUnitOfWork unitOfWork,
    IMediator mediator) : IRequestHandler<AssignUserFeaturesCommand, IReadOnlyList<string>>
{
    public async Task<IReadOnlyList<string>> Handle(AssignUserFeaturesCommand request, CancellationToken cancellationToken)
    {
        _ = await subscriptionRepository.GetByTenantIdAsync(request.TenantId, cancellationToken)
            ?? throw new DomainException("Subscription not found.");

        if (request.FeatureKeys is null || request.FeatureKeys.Count == 0)
            throw new DomainException("At least one feature key is required.");

        var effectiveEntitlements = await mediator.Send(new Queries.GetEffectiveEntitlements.GetEffectiveEntitlementsQuery(request.TenantId), cancellationToken);
        var grantedFeatures = effectiveEntitlements
            .Where(x => x.Granted)
            .ToDictionary(x => x.FeatureKey, StringComparer.OrdinalIgnoreCase);

        var assigned = new List<string>();
        foreach (var rawFeatureKey in request.FeatureKeys.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!grantedFeatures.TryGetValue(rawFeatureKey, out _))
                throw new DomainException($"Tenant does not have feature '{rawFeatureKey}'.");

            var feature = await featureCatalogRepository.GetByFeatureKeyAsync(rawFeatureKey, cancellationToken)
                ?? throw new DomainException($"Feature '{rawFeatureKey}' was not found in the catalog.");

            if (feature.AssignmentMode == FeatureAssignmentMode.TenantWide)
                throw new DomainException($"Feature '{rawFeatureKey}' is tenant-wide and does not require per-user assignment.");

            var existing = await assignmentRepository.GetActiveAssignmentAsync(request.TenantId, request.UserId, rawFeatureKey, cancellationToken);
            if (existing is not null)
                continue;

            if (feature.AssignmentMode == FeatureAssignmentMode.SeatLimitedAssignment && feature.DefaultAssignmentLimit.HasValue)
            {
                var activeCount = await assignmentRepository.CountActiveAssignmentsAsync(request.TenantId, rawFeatureKey, cancellationToken);
                if (activeCount >= feature.DefaultAssignmentLimit.Value)
                    throw new DomainException($"Feature '{rawFeatureKey}' has reached its assignment limit.");
            }

            var assignment = TenantUserFeatureAssignment.Create(
                request.TenantId,
                request.UserId,
                rawFeatureKey,
                request.AssignedByUserId,
                request.EffectiveFrom ?? DateTimeOffset.UtcNow,
                request.EffectiveTo,
                request.Notes,
                request.MetadataJson);

            await assignmentRepository.AddAsync(assignment, cancellationToken);
            assigned.Add(rawFeatureKey);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return assigned;
    }
}
