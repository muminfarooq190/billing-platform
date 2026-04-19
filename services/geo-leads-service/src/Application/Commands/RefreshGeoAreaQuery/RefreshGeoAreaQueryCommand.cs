using MediatR;

namespace GeoLeadsService.Application.Commands.RefreshGeoAreaQuery;

public sealed record RefreshGeoAreaQueryCommand(Guid TenantId, Guid QueryId) : IRequest<(Guid QueryId, int Count)?>;
