namespace IdentityService.Application.ReadModels;

public sealed record TenantReadModel(
    Guid Id,
    string Name,
    string Email,
    string Plan,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
