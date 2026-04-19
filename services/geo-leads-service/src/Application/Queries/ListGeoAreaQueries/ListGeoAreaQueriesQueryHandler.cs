using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using MediatR;

namespace GeoLeadsService.Application.Queries.ListGeoAreaQueries;

public sealed class ListGeoAreaQueriesQueryHandler(IGeoAreaQueryRepository geoAreaQueryRepository, IFeatureGate featureGate) : IRequestHandler<ListGeoAreaQueriesQuery, IReadOnlyList<GeoAreaQuery>>
{
    public async Task<IReadOnlyList<GeoAreaQuery>> Handle(ListGeoAreaQueriesQuery request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync("geo-leads.read", request.TenantId, cancellationToken);
        return await geoAreaQueryRepository.ListByTenantAsync(request.TenantId, request.Limit, cancellationToken);
    }
}
