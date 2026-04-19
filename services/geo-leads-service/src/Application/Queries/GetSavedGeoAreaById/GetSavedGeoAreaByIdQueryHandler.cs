using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using MediatR;

namespace GeoLeadsService.Application.Queries.GetSavedGeoAreaById;

public sealed class GetSavedGeoAreaByIdQueryHandler(ISavedGeoAreaRepository savedGeoAreaRepository, IFeatureGate featureGate) : IRequestHandler<GetSavedGeoAreaByIdQuery, SavedGeoArea?>
{
    public async Task<SavedGeoArea?> Handle(GetSavedGeoAreaByIdQuery request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync("geo-leads.read", request.TenantId, cancellationToken);
        return await savedGeoAreaRepository.GetByIdAsync(request.AreaId, request.TenantId, cancellationToken);
    }
}
