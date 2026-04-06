namespace TravelService.Api.Contracts;

public sealed record UpdateContactRequest(
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? Company,
    string? Notes,
    List<string>? Tags);
