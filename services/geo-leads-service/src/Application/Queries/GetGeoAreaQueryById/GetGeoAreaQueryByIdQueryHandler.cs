using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using MediatR;

namespace GeoLeadsService.Application.Queries.GetGeoAreaQueryById;

public sealed class GetGeoAreaQueryByIdQueryHandler(
    IGeoAreaQueryRepository geoAreaQueryRepository,
    IGeoAreaQueryResultStore geoAreaQueryResultStore) : IRequestHandler<GetGeoAreaQueryByIdQuery, GeoAreaQuery?>
{
    public async Task<GeoAreaQuery?> Handle(GetGeoAreaQueryByIdQuery request, CancellationToken cancellationToken)
    {
        var query = await geoAreaQueryRepository.GetByIdAsync(request.QueryId, request.TenantId, cancellationToken);
        if (query is null)
            return null;

        var results = await geoAreaQueryResultStore.GetAsync(request.QueryId, cancellationToken);
        query.Complete(results);
        return query;
    }
}
