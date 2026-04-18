using GeoLeadsService.Application.Abstractions;
using MediatR;

namespace GeoLeadsService.Application.Commands.SubmitGeoAreaQuery;

public sealed record SubmitGeoAreaQueryCommand(
    Guid TenantId,
    GeoPolygon Geometry,
    IReadOnlyList<string> LeadTypes,
    int Limit,
    string? RankingMode) : IRequest<(Guid QueryId, int Count)>;
