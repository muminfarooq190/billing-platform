using TravelService.Application.Queries.GetItineraryById;
using MediatR;

namespace TravelService.Application.Queries.ListItinerariesByTenant;

public sealed record ListItinerariesByTenantQuery(Guid TenantId) : IRequest<IReadOnlyList<ItineraryReadModel>>;
