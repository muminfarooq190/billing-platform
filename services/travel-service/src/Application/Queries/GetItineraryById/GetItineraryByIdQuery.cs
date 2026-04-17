using MediatR;

namespace TravelService.Application.Queries.GetItineraryById;

public sealed record GetItineraryByIdQuery(Guid Id) : IRequest<ItineraryReadModel?>;

public sealed record ItineraryReadModel(
    Guid Id,
    Guid TenantId,
    Guid CustomerContactId,
    string CustomerName,
    string Title,
    string Destination,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    int Travellers,
    string Currency,
    Guid? QuotationId,
    Guid? BookingId,
    string Status,
    decimal TotalCost,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
