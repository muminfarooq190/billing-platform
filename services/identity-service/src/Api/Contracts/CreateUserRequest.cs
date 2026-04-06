namespace IdentityService.Api.Contracts;

public sealed record CreateUserRequest(
    Guid TenantId,
    string Email,
    string Password,
    string Role);
