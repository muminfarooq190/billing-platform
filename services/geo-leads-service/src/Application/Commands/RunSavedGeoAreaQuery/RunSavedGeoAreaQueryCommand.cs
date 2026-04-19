using MediatR;

namespace GeoLeadsService.Application.Commands.RunSavedGeoAreaQuery;

public sealed record RunSavedGeoAreaQueryCommand(Guid TenantId, Guid AreaId, IReadOnlyList<string> LeadTypes, int Limit, string? RankingMode) : IRequest<(Guid QueryId, int Count)?>;
