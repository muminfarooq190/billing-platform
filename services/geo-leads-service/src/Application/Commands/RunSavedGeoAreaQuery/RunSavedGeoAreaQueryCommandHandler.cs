using System.Text.Json;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Application.Commands.SubmitGeoAreaQuery;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using MediatR;

namespace GeoLeadsService.Application.Commands.RunSavedGeoAreaQuery;

public sealed class RunSavedGeoAreaQueryCommandHandler(ISavedGeoAreaRepository savedGeoAreaRepository, IFeatureGate featureGate, IMediator mediator) : IRequestHandler<RunSavedGeoAreaQueryCommand, (Guid QueryId, int Count)?>
{
    public async Task<(Guid QueryId, int Count)?> Handle(RunSavedGeoAreaQueryCommand request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync("geo-leads.manage", request.TenantId, cancellationToken);

        var area = await savedGeoAreaRepository.GetByIdAsync(request.AreaId, request.TenantId, cancellationToken);
        if (area is null)
            return null;

        var polygon = JsonSerializer.Deserialize<GeoPolygon>(area.GeometryJson);
        if (polygon is null)
            return null;

        return await mediator.Send(new SubmitGeoAreaQueryCommand(request.TenantId, polygon, request.LeadTypes, request.Limit, request.RankingMode), cancellationToken);
    }
}
