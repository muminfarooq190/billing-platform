using GeoLeadsService.Domain.Aggregates;
using MediatR;

namespace GeoLeadsService.Application.Queries.ListSavedGeoAreas;

public sealed record ListSavedGeoAreasQuery(Guid TenantId, int Limit) : IRequest<IReadOnlyList<SavedGeoArea>>;
