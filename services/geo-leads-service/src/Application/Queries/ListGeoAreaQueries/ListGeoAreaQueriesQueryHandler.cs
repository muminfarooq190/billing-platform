using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Repositories;
using MediatR;

namespace GeoLeadsService.Application.Queries.ListGeoAreaQueries;

public sealed class ListGeoAreaQueriesQueryHandler(IGeoAreaQueryRepository geoAreaQueryRepository, IFeatureGate featureGate) : IRequestHandler<ListGeoAreaQueriesQuery, IReadOnlyList<GeoAreaQueryListItem>>
{
    public async Task<IReadOnlyList<GeoAreaQueryListItem>> Handle(ListGeoAreaQueriesQuery request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync("geo-leads.read", request.TenantId, cancellationToken);
        return await geoAreaQueryRepository.ListSummariesByTenantAsync(request.TenantId, request.Limit, cancellationToken);
    }
}
