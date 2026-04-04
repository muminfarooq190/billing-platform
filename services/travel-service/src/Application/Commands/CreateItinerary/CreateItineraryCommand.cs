using MediatR;

namespace TravelService.Application.Commands.CreateItinerary;

public sealed record CreateItineraryCommand(
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
    List<ItineraryItemDto> Items) : IRequest<Guid>;

public sealed record ItineraryItemDto(
    int DayNumber,
    string ItemType,
    string Title,
    string Description,
    string Location,
    DateTimeOffset? StartTime,
    DateTimeOffset? EndTime,
    decimal Cost,
    string Currency);
