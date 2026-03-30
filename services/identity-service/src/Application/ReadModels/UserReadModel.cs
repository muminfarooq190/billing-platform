namespace IdentityService.Application.ReadModels;

public sealed record UserReadModel(
    Guid Id,
    Guid TenantId,
    string Email,
    string Role,
    DateTimeOffset CreatedAt);
