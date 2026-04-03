using Dapper;
using IdentityService.Application.Abstractions;
using IdentityService.Application.ReadModels;
using MediatR;

namespace IdentityService.Application.Queries.GetTenantByEmail;

public sealed class GetTenantByEmailQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<GetTenantByEmailQuery, TenantReadModel?>
{
    public async Task<TenantReadModel?> Handle(GetTenantByEmailQuery request, CancellationToken cancellationToken)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        const string sql = """
            SELECT "Id" AS Id,
                   "Name" AS Name,
                   "Email" AS Email,
                   "Plan"::text AS Plan,
                   "Status"::text AS Status,
                   created_at AS CreatedAt,
                   COALESCE(updated_at, created_at) AS UpdatedAt
            FROM tenants
            WHERE "Email" = @Email AND deleted_at IS NULL;
            """;

        return await connection.QuerySingleOrDefaultAsync<TenantReadModel>(new CommandDefinition(sql, new { request.Email }, cancellationToken: cancellationToken));
    }
}
