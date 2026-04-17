namespace TravelService.Api.Contracts;

public sealed record CreateBookingItineraryRequest(
    string Title,
    string Destination,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    int Travellers,
    string Currency,
    List<ItineraryItemRequest> Items);
