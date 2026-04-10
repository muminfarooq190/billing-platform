namespace IdentityService.Application.ReadModels;

public sealed record UserReadModel(
    Guid Id,
    Guid TenantId,
    string Email,
    string Role,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset? PasswordChangedAt,
    bool MustChangePassword);
