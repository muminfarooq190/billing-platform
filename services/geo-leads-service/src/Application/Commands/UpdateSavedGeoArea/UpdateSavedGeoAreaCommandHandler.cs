using System.Text.Json;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using MediatR;

namespace GeoLeadsService.Application.Commands.UpdateSavedGeoArea;

public sealed class UpdateSavedGeoAreaCommandHandler(ISavedGeoAreaRepository savedGeoAreaRepository, IFeatureGate featureGate) : IRequestHandler<UpdateSavedGeoAreaCommand, SavedGeoArea?>
{
    public async Task<SavedGeoArea?> Handle(UpdateSavedGeoAreaCommand request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync("geo-leads.manage", request.TenantId, cancellationToken);

        var area = await savedGeoAreaRepository.GetByIdAsync(request.AreaId, request.TenantId, cancellationToken);
        if (area is null)
            return null;

        area.Update(request.Name, JsonSerializer.Serialize(request.Geometry));
        await savedGeoAreaRepository.UpdateAsync(area, cancellationToken);
        return area;
    }
}
