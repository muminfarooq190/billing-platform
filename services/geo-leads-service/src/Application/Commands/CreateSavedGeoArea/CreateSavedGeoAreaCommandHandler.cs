using System.Text.Json;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using MediatR;

namespace GeoLeadsService.Application.Commands.CreateSavedGeoArea;

public sealed class CreateSavedGeoAreaCommandHandler(ISavedGeoAreaRepository savedGeoAreaRepository, IFeatureGate featureGate) : IRequestHandler<CreateSavedGeoAreaCommand, SavedGeoArea>
{
    public async Task<SavedGeoArea> Handle(CreateSavedGeoAreaCommand request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync("geo-leads.manage", request.TenantId, cancellationToken);

        var area = new SavedGeoArea(request.TenantId, request.Name, JsonSerializer.Serialize(request.Geometry));
        await savedGeoAreaRepository.AddAsync(area, cancellationToken);
        return area;
    }
}
