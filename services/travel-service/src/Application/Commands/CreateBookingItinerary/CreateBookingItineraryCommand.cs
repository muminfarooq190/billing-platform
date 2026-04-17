using MediatR;
using TravelService.Application.Commands.CreateItinerary;

namespace TravelService.Application.Commands.CreateBookingItinerary;

public sealed record CreateBookingItineraryCommand(
    Guid TenantId,
    Guid BookingId,
    string Title,
    string Destination,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    int Travellers,
    string Currency,
    List<ItineraryItemDto> Items) : IRequest<Guid>;
