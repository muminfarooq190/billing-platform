using System.Text.Json;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using MediatR;

namespace GeoLeadsService.Application.Commands.SubmitGeoAreaQuery;

public sealed class SubmitGeoAreaQueryCommandHandler(
    IGeoAreaQueryRepository geoAreaQueryRepository,
    IGeoLeadCatalog geoLeadCatalog,
    IFeatureGate featureGate) : IRequestHandler<SubmitGeoAreaQueryCommand, (Guid QueryId, int Count)>
{
    public async Task<(Guid QueryId, int Count)> Handle(SubmitGeoAreaQueryCommand request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync("geo-leads.manage", request.TenantId, cancellationToken);
        var query = new GeoAreaQuery(
            request.TenantId,
            JsonSerializer.Serialize(request.Geometry),
            request.LeadTypes,
            request.Limit,
            request.RankingMode);

        var leads = await geoLeadCatalog.SearchAsync(request.Geometry, request.LeadTypes, request.Limit, cancellationToken);
        var rankedLeads = SpatialLeadScoring.Rank(request.Geometry, leads, query.RankingMode);

        var results = rankedLeads.Select((entry, index) => new GeoAreaQueryResult(
            query.Id,
            index + 1,
            entry.Score,
            entry.Lead,
            [.. entry.Lead.Reasons, $"distanceMeters={entry.DistanceMeters:0.##}", $"intersectionBoost={entry.IntersectionBoost:0.####}"]))
            .ToList();

        query.Complete(results);
        await geoAreaQueryRepository.AddAsync(query, cancellationToken);
        return (query.Id, results.Count);
    }
}
