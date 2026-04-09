using MediatR;

namespace TravelService.Application.Queries.SearchTravel;

public sealed record SearchTravelQuery(Guid TenantId, string Query, int Page = 1, int PageSize = 20) : IRequest<IReadOnlyList<SearchResultReadModel>>;

public sealed record SearchResultReadModel(
    string EntityType,
    Guid EntityId,
    string Title,
    string Subtitle,
    DateTimeOffset? RelevantDate);
