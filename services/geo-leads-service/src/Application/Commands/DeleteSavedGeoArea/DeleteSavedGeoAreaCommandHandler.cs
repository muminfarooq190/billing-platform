using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Repositories;
using MediatR;

namespace GeoLeadsService.Application.Commands.DeleteSavedGeoArea;

public sealed class DeleteSavedGeoAreaCommandHandler(ISavedGeoAreaRepository savedGeoAreaRepository, IFeatureGate featureGate) : IRequestHandler<DeleteSavedGeoAreaCommand, bool>
{
    public async Task<bool> Handle(DeleteSavedGeoAreaCommand request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync("geo-leads.manage", request.TenantId, cancellationToken);

        var area = await savedGeoAreaRepository.GetByIdAsync(request.AreaId, request.TenantId, cancellationToken);
        if (area is null)
            return false;

        await savedGeoAreaRepository.DeleteAsync(area, cancellationToken);
        return true;
    }
}
