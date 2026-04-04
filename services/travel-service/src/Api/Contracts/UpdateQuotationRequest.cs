namespace TravelService.Api.Contracts;

public sealed record UpdateQuotationRequest(
    string Title,
    string Destination,
    DateTimeOffset TravelDate,
    DateTimeOffset ReturnDate,
    int Travellers,
    string Currency,
    string Notes,
    DateTimeOffset ValidUntil,
    string? Action);
