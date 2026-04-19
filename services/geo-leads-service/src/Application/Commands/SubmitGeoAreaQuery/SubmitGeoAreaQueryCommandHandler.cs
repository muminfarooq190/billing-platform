using System.Text.Json;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using MediatR;

namespace GeoLeadsService.Application.Commands.SubmitGeoAreaQuery;

public sealed class SubmitGeoAreaQueryCommandHandler(
    IGeoAreaQueryRepository geoAreaQueryRepository,
    IGeoLeadCatalog geoLeadCatalog) : IRequestHandler<SubmitGeoAreaQueryCommand, (Guid QueryId, int Count)>
{
    public async Task<(Guid QueryId, int Count)> Handle(SubmitGeoAreaQueryCommand request, CancellationToken cancellationToken)
    {
        var query = new GeoAreaQuery(
            request.TenantId,
            JsonSerializer.Serialize(request.Geometry),
            request.LeadTypes,
            request.Limit,
            request.RankingMode);

        var leads = await geoLeadCatalog.SearchAsync(request.Geometry, request.LeadTypes, request.Limit, cancellationToken);
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
        await geoAreaQueryRepository.AddAsync(query, cancellationToken);
        return (query.Id, results.Count);
    }
}
