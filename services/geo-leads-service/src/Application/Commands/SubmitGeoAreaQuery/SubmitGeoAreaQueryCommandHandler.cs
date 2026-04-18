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
            request.Limit);

        var leads = await geoLeadCatalog.SearchAsync(request.Geometry, request.LeadTypes, request.Limit, cancellationToken);
        var results = leads.Select((lead, index) => new GeoAreaQueryResult(
            query.Id,
            index + 1,
            Math.Round((lead.TourismRelevanceScore * 0.35m) + (lead.ContactabilityScore * 0.30m) + (lead.ConfidenceScore * 0.35m), 4),
            lead,
            lead.Reasons)).ToList();

        query.Complete(results);
        await geoAreaQueryRepository.AddAsync(query, cancellationToken);
        return (query.Id, results.Count);
    }
}
