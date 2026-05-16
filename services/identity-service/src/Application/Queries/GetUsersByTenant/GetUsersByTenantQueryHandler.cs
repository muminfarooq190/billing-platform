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
        // Postgres column names are mixed-case (quoted) for EF entity props ("Id",
        // "TenantId", "Email", "Role", "Status") and snake_case for value-object/
        // domain timestamps. Quote PascalCase identifiers exactly.
        const string sql = """
            SELECT "Id" AS id,
                   "TenantId" AS TenantId,
                   "Email" AS email,
                   "Role"::text AS role,
                   "Status"::text AS status,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt,
                   last_login_at AS LastLoginAt,
                   password_changed_at AS PasswordChangedAt,
                   must_change_password AS MustChangePassword
            FROM users
            WHERE "TenantId" = @TenantId AND deleted_at IS NULL
            ORDER BY created_at DESC;
            """;

        var users = await connection.QueryAsync<UserReadModel>(new CommandDefinition(sql, new { request.TenantId }, cancellationToken: cancellationToken));
        return users.ToList();
    }
}
