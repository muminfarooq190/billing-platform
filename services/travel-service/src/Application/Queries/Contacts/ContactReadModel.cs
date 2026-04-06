namespace TravelService.Application.Queries.Contacts;

public sealed record ContactReadModel(
    Guid Id,
    Guid TenantId,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Company,
    string Notes,
    string Tags,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
