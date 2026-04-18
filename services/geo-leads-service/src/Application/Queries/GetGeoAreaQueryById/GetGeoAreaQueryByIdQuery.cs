using GeoLeadsService.Domain.Aggregates;
using MediatR;

namespace GeoLeadsService.Application.Queries.GetGeoAreaQueryById;

public sealed record GetGeoAreaQueryByIdQuery(Guid TenantId, Guid QueryId) : IRequest<GeoAreaQuery?>;
