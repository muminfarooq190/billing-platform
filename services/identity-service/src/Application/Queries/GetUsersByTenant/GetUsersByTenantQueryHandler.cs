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
            SELECT id, tenant_id AS TenantId, email, role::text AS role, created_at AS CreatedAt
            FROM users
            WHERE tenant_id = @TenantId AND deleted_at IS NULL
            ORDER BY created_at DESC;
            """;

        var users = await connection.QueryAsync<UserReadModel>(new CommandDefinition(sql, new { request.TenantId }, cancellationToken: cancellationToken));
        return users.ToList();
    }
}
