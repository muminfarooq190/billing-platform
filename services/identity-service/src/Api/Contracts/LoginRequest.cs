namespace IdentityService.Api.Contracts;

public sealed record LoginRequest(string Email, string Password, string? MfaCode = null);
