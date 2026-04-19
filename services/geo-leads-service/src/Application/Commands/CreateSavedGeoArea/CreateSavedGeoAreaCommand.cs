using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;
using MediatR;

namespace GeoLeadsService.Application.Commands.CreateSavedGeoArea;

public sealed record CreateSavedGeoAreaCommand(Guid TenantId, string Name, GeoPolygon Geometry) : IRequest<SavedGeoArea>;
