namespace IdentityService.Api.Contracts;

public sealed record RegisterRequest(string TenantName, string Email, string Password);
