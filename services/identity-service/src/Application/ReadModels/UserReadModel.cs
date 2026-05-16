namespace IdentityService.Application.ReadModels;

// Plain class (settable props + parameterless ctor) so Dapper uses property
// binding instead of positional ctor matching. Required because Npgsql 8 returns
// DateTime for timestamptz, which doesn't match a positional DateTimeOffset ctor.
public sealed class UserReadModel
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public DateTimeOffset? PasswordChangedAt { get; set; }
    public bool MustChangePassword { get; set; }
}
