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
        var rankedLeads = leads
            .Select(lead => new { Lead = lead, Score = GeoLeadRanking.Score(lead, query.RankingMode) })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Lead.CanonicalName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var results = rankedLeads.Select((entry, index) => new GeoAreaQueryResult(
            query.Id,
            index + 1,
            entry.Score,
            entry.Lead,
            entry.Lead.Reasons)).ToList();

        query.Complete(results);
        await geoAreaQueryRepository.UpdateAsync(query, cancellationToken);
        return (query.Id, results.Count);
    }
}
