using TravelService.Application.Queries.GetItineraryById;
using MediatR;

namespace TravelService.Application.Queries.ListItinerariesByTenant;

public sealed record ListItinerariesByTenantQuery(
    Guid TenantId,
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    string? CustomerName = null,
    DateTimeOffset? StartDateFrom = null,
    DateTimeOffset? StartDateTo = null,
    Guid? BookingId = null,
    Guid? QuotationId = null,
    string? OwnershipType = null)
    : IRequest<IReadOnlyList<ItineraryReadModel>>;
