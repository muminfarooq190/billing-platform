using BillingService.Application.Abstractions;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using MediatR;

namespace BillingService.Application.Commands.CreateTenantFeatureOverride;

public sealed class CreateTenantFeatureOverrideCommandHandler(
    ITenantFeatureOverrideRepository tenantFeatureOverrideRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateTenantFeatureOverrideCommand, Guid>
{
    public async Task<Guid> Handle(CreateTenantFeatureOverrideCommand request, CancellationToken cancellationToken)
    {
        var entry = TenantFeatureOverride.Create(
            request.TenantId,
            request.FeatureKey,
            request.Granted,
            request.Reason,
            request.Source,
            request.LimitValue,
            request.CreatedBy,
            request.EffectiveFrom,
            request.EffectiveTo,
            request.MetadataJson is null ? null : new { Raw = request.MetadataJson });

        await tenantFeatureOverrideRepository.AddRangeAsync([entry], cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entry.Id;
    }
}
