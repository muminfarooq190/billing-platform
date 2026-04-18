using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using MediatR;

namespace GeoLeadsService.Application.Queries.GetGeoAreaQueryById;

public sealed class GetGeoAreaQueryByIdQueryHandler(IGeoAreaQueryRepository geoAreaQueryRepository) : IRequestHandler<GetGeoAreaQueryByIdQuery, GeoAreaQuery?>
{
    public Task<GeoAreaQuery?> Handle(GetGeoAreaQueryByIdQuery request, CancellationToken cancellationToken)
        => geoAreaQueryRepository.GetByIdAsync(request.QueryId, request.TenantId, cancellationToken);
}
