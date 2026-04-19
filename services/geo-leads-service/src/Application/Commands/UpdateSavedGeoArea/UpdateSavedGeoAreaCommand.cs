using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;
using MediatR;

namespace GeoLeadsService.Application.Commands.UpdateSavedGeoArea;

public sealed record UpdateSavedGeoAreaCommand(Guid TenantId, Guid AreaId, string Name, GeoPolygon Geometry) : IRequest<SavedGeoArea?>;
