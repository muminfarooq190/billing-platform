using MediatR;

namespace TravelService.Application.Commands.UpdateItinerary;

public sealed record UpdateItineraryCommand(
    Guid Id,
    string Title,
    string Destination,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    int Travellers,
    string Currency,
    string? Action) : IRequest;
