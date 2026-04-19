using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using MediatR;

namespace GeoLeadsService.Application.Queries.ListSavedGeoAreas;

public sealed class ListSavedGeoAreasQueryHandler(ISavedGeoAreaRepository savedGeoAreaRepository, IFeatureGate featureGate) : IRequestHandler<ListSavedGeoAreasQuery, IReadOnlyList<SavedGeoArea>>
{
    public async Task<IReadOnlyList<SavedGeoArea>> Handle(ListSavedGeoAreasQuery request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync("geo-leads.read", request.TenantId, cancellationToken);
        return await savedGeoAreaRepository.ListByTenantAsync(request.TenantId, request.Limit, cancellationToken);
    }
}
