namespace IdentityService.Api.Contracts;

public sealed record CreateRoleRequest(string Name, string Description, IReadOnlyList<string> PermissionKeys);
public sealed record UpdateRoleRequest(string Name, string Description, IReadOnlyList<string> PermissionKeys);
public sealed record UpdateUserRolesRequest(IReadOnlyList<Guid> RoleIds);
public sealed record MfaEnrollRequest(string? DeviceName);
public sealed record MfaVerifyRequest(string Code);
public sealed record MfaDisableRequest(string Code);
