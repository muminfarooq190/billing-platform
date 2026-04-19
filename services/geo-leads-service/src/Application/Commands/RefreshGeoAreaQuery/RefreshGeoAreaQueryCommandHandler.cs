using System.Text.Json;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using MediatR;

namespace GeoLeadsService.Application.Commands.RefreshGeoAreaQuery;

public sealed class RefreshGeoAreaQueryCommandHandler(
    IGeoAreaQueryRepository geoAreaQueryRepository,
    IGeoLeadCatalog geoLeadCatalog) : IRequestHandler<RefreshGeoAreaQueryCommand, (Guid QueryId, int Count)?>
{
    public async Task<(Guid QueryId, int Count)?> Handle(RefreshGeoAreaQueryCommand request, CancellationToken cancellationToken)
    {
        var query = await geoAreaQueryRepository.GetByIdAsync(request.QueryId, request.TenantId, cancellationToken);
        if (query is null)
            return null;

        var polygon = JsonSerializer.Deserialize<GeoPolygon>(query.GeometryJson);
        if (polygon is null)
            return null;

        var leadTypes = JsonSerializer.Deserialize<List<string>>(query.RequestedLeadTypesJson) ?? [];
        var leads = await geoLeadCatalog.SearchAsync(polygon, leadTypes, query.RequestedLimit, cancellationToken);
        var results = leads.Select((lead, index) => new GeoAreaQueryResult(
            query.Id,
            index + 1,
            Math.Round((lead.TourismRelevanceScore * 0.35m) + (lead.ContactabilityScore * 0.30m) + (lead.ConfidenceScore * 0.35m), 4),
            lead,
            lead.Reasons)).ToList();

        query.Complete(results);
        await geoAreaQueryRepository.UpdateAsync(query, cancellationToken);
        return (query.Id, results.Count);
    }
}
