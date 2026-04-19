using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using MediatR;

namespace GeoLeadsService.Application.Queries.GetGeoAreaQueryById;

public sealed class GetGeoAreaQueryByIdQueryHandler(IGeoAreaQueryRepository geoAreaQueryRepository, IFeatureGate featureGate) : IRequestHandler<GetGeoAreaQueryByIdQuery, GeoAreaQuery?>
{
    public async Task<GeoAreaQuery?> Handle(GetGeoAreaQueryByIdQuery request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync("geo-leads.read", request.TenantId, cancellationToken);
        return await geoAreaQueryRepository.GetByIdAsync(request.QueryId, request.TenantId, cancellationToken);
    }
}
