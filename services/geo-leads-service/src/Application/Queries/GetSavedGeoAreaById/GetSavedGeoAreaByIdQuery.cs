using GeoLeadsService.Domain.Aggregates;
using MediatR;

namespace GeoLeadsService.Application.Queries.GetSavedGeoAreaById;

public sealed record GetSavedGeoAreaByIdQuery(Guid TenantId, Guid AreaId) : IRequest<SavedGeoArea?>;
