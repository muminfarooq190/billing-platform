using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using MediatR;

namespace GeoLeadsService.Application.Queries.ListGeoAreaQueries;

public sealed class ListGeoAreaQueriesQueryHandler(IGeoAreaQueryRepository geoAreaQueryRepository) : IRequestHandler<ListGeoAreaQueriesQuery, IReadOnlyList<GeoAreaQuery>>
{
    public Task<IReadOnlyList<GeoAreaQuery>> Handle(ListGeoAreaQueriesQuery request, CancellationToken cancellationToken)
        => geoAreaQueryRepository.ListByTenantAsync(request.TenantId, request.Limit, cancellationToken);
}
