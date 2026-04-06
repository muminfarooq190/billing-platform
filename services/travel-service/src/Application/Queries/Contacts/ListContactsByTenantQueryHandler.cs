using Dapper;
using MediatR;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Queries.Contacts;

public sealed class ListContactsByTenantQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<ListContactsByTenantQuery, PagedResult<ContactReadModel>>
{
    public async Task<PagedResult<ContactReadModel>> Handle(ListContactsByTenantQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var offset = (page - 1) * pageSize;

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;

        var totalCount = await dbConnection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM contacts WHERE tenant_id = @TenantId AND deleted_at IS NULL",
            new { request.TenantId });

        var items = await dbConnection.QueryAsync<ContactReadModel>(
            "SELECT id, tenant_id AS TenantId, first_name AS FirstName, last_name AS LastName, email, phone, company, notes, tags, created_at AS CreatedAt, updated_at AS UpdatedAt FROM contacts WHERE tenant_id = @TenantId AND deleted_at IS NULL ORDER BY created_at DESC OFFSET @Offset LIMIT @Limit",
            new { request.TenantId, Offset = offset, Limit = pageSize });

        return new PagedResult<ContactReadModel>(items.ToList().AsReadOnly(), page, pageSize, totalCount);
    }
}
