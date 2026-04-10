using Dapper;
using IdentityService.Application.Abstractions;
using IdentityService.Application.ReadModels;
using MediatR;

namespace IdentityService.Application.Queries.GetUsersByTenant;

public sealed class GetUsersByTenantQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<GetUsersByTenantQuery, IReadOnlyList<UserReadModel>>
{
    public async Task<IReadOnlyList<UserReadModel>> Handle(GetUsersByTenantQuery request, CancellationToken cancellationToken)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        const string sql = """
            SELECT id,
                   tenant_id AS TenantId,
                   email,
                   role::text AS role,
                   status::text AS status,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt,
                   last_login_at AS LastLoginAt,
                   password_changed_at AS PasswordChangedAt,
                   must_change_password AS MustChangePassword
            FROM users
            WHERE tenant_id = @TenantId AND deleted_at IS NULL
            ORDER BY created_at DESC;
            """;

        var users = await connection.QueryAsync<UserReadModel>(new CommandDefinition(sql, new { request.TenantId }, cancellationToken: cancellationToken));
        return users.ToList();
    }
}
