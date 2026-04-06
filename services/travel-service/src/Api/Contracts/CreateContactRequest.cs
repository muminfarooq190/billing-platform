namespace TravelService.Api.Contracts;

public sealed record CreateContactRequest(
    Guid TenantId,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? Company,
    string? Notes,
    List<string>? Tags);
