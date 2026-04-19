using System.Text.Json;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using MediatR;

namespace GeoLeadsService.Application.Commands.RefreshGeoAreaQuery;

public sealed class RefreshGeoAreaQueryCommandHandler(
    IGeoAreaQueryRepository geoAreaQueryRepository,
    IGeoLeadCatalog geoLeadCatalog,
    IFeatureGate featureGate) : IRequestHandler<RefreshGeoAreaQueryCommand, (Guid QueryId, int Count)?>
{
    public async Task<(Guid QueryId, int Count)?> Handle(RefreshGeoAreaQueryCommand request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync("geo-leads.manage", request.TenantId, cancellationToken);
        var query = await geoAreaQueryRepository.GetByIdAsync(request.QueryId, request.TenantId, cancellationToken);
        if (query is null)
            return null;

        var polygon = JsonSerializer.Deserialize<GeoPolygon>(query.GeometryJson);
        if (polygon is null)
            return null;

        var leadTypes = JsonSerializer.Deserialize<List<string>>(query.RequestedLeadTypesJson) ?? [];
        var leads = await geoLeadCatalog.SearchAsync(polygon, leadTypes, query.RequestedLimit, cancellationToken);
        var rankedLeads = SpatialLeadScoring.Rank(polygon, leads, query.RankingMode);

        var results = rankedLeads.Select((entry, index) => new GeoAreaQueryResult(
            query.Id,
            index + 1,
            entry.Score,
            entry.Lead,
            [.. entry.Lead.Reasons, $"distanceMeters={entry.DistanceMeters:0.##}", $"intersectionBoost={entry.IntersectionBoost:0.####}"]))
            .ToList();

        query.Complete(results);
        await geoAreaQueryRepository.UpdateAsync(query, cancellationToken);
        return (query.Id, results.Count);
    }
}
