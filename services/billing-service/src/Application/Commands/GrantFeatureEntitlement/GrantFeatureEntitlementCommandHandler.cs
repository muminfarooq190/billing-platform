using BillingService.Application.Abstractions;
using BillingService.Application.ReadModels;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Enums;
using BillingService.Domain.Repositories;
using MediatR;

namespace BillingService.Application.Commands.GrantFeatureEntitlement;

public sealed class GrantFeatureEntitlementCommandHandler(IFeatureEntitlementRepository featureEntitlementRepository, IUnitOfWork unitOfWork) : IRequestHandler<GrantFeatureEntitlementCommand, FeatureEntitlementReadModel>
{
    public async Task<FeatureEntitlementReadModel> Handle(GrantFeatureEntitlementCommand request, CancellationToken cancellationToken)
    {
        var entry = FeatureEntitlement.Create(
            request.TenantId,
            request.FeatureKey,
            request.Granted,
            EntitlementSource.AdminGrant,
            null,
            request.LimitValue,
            request.EffectiveFrom,
            request.EffectiveTo,
            new { request.Reason });

        await featureEntitlementRepository.AddRangeAsync([entry], cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new FeatureEntitlementReadModel
        {
            FeatureKey = entry.FeatureKey,
            Granted = entry.Granted,
            Source = entry.Source.ToString(),
            PlanType = entry.PlanType?.ToString(),
            LimitValue = entry.LimitValue,
            EffectiveFrom = entry.EffectiveFrom,
            EffectiveTo = entry.EffectiveTo,
            MetadataJson = entry.MetadataJson
        };
    }
}
