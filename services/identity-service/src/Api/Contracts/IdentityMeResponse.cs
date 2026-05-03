namespace IdentityService.Api.Contracts;

public sealed record IdentityMeResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    bool IsActive,
    Guid? TenantId,
    string? TenantName,
    IReadOnlyList<string>? RoleKeys,
    IReadOnlyList<string>? Permissions
);
