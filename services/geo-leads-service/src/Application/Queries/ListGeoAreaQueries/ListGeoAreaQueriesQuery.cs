using MediatR;

namespace GeoLeadsService.Application.Queries.ListGeoAreaQueries;

public sealed record ListGeoAreaQueriesQuery(Guid TenantId, int Limit) : IRequest<IReadOnlyList<GeoAreaQueryListItem>>;
