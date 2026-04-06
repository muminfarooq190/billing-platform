namespace IdentityService.Api.Contracts;

public sealed record UpdateUserRequest(
    string Role,
    string? Password);
