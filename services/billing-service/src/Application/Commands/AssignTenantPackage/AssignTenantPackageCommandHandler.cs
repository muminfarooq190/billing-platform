using BillingService.Application.Abstractions;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using MediatR;

namespace BillingService.Application.Commands.AssignTenantPackage;

public sealed class AssignTenantPackageCommandHandler(
    ICommercialPackageRepository commercialPackageRepository,
    ITenantSubscriptionPackageRepository tenantSubscriptionPackageRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AssignTenantPackageCommand, Guid>
{
    public async Task<Guid> Handle(AssignTenantPackageCommand request, CancellationToken cancellationToken)
    {
        var package = (await commercialPackageRepository.ListActiveAsync(cancellationToken))
            .FirstOrDefault(x => x.Id == request.CommercialPackageId);
        if (package is null)
        {
            throw new InvalidOperationException($"Commercial package '{request.CommercialPackageId}' was not found.");
        }

        var assignment = TenantSubscriptionPackage.Create(
            request.TenantId,
            request.CommercialPackageId,
            request.Source,
            request.Status,
            request.EffectiveFrom,
            request.EffectiveTo,
            request.MetadataJson);

        await tenantSubscriptionPackageRepository.AddRangeAsync([assignment], cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return assignment.Id;
    }
}
