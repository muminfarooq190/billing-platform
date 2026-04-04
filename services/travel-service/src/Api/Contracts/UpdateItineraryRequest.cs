namespace TravelService.Api.Contracts;

public sealed record UpdateItineraryRequest(
    string Title,
    string Destination,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    int Travellers,
    string Currency,
    string? Action);
