using Dapper;
using IdentityService.Application.Abstractions;
using IdentityService.Application.ReadModels;
using MediatR;

namespace IdentityService.Application.Queries.GetTenantById;

public sealed class GetTenantByIdQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<GetTenantByIdQuery, TenantReadModel?>
{
    public async Task<TenantReadModel?> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        const string sql = """
            SELECT id, name, email, plan::text AS plan, status::text AS status, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM tenants
            WHERE id = @TenantId AND deleted_at IS NULL;
            """;

        return await connection.QuerySingleOrDefaultAsync<TenantReadModel>(new CommandDefinition(sql, new { request.TenantId }, cancellationToken: cancellationToken));
    }
}
