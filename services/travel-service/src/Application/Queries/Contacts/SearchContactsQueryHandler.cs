using Dapper;
using MediatR;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Queries.Contacts;

public sealed class SearchContactsQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<SearchContactsQuery, PagedResult<ContactReadModel>>
{
    public async Task<PagedResult<ContactReadModel>> Handle(SearchContactsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var offset = (page - 1) * pageSize;
        var searchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : $"%{request.SearchTerm.Trim()}%";

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;

        const string whereClause = "tenant_id = @TenantId AND deleted_at IS NULL AND (@SearchTerm IS NULL OR first_name ILIKE @SearchTerm OR last_name ILIKE @SearchTerm OR email ILIKE @SearchTerm OR phone ILIKE @SearchTerm OR company ILIKE @SearchTerm OR EXISTS (SELECT 1 FROM unnest(tags) AS tag WHERE tag ILIKE @SearchTerm))";

        var totalCount = await dbConnection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM contacts WHERE {whereClause}",
            new { request.TenantId, SearchTerm = searchTerm });

        var items = await dbConnection.QueryAsync<ContactReadModel>(
            $"SELECT id, tenant_id AS TenantId, first_name AS FirstName, last_name AS LastName, email, phone, company, notes, tags, created_at AS CreatedAt, updated_at AS UpdatedAt FROM contacts WHERE {whereClause} ORDER BY created_at DESC OFFSET @Offset LIMIT @Limit",
            new { request.TenantId, SearchTerm = searchTerm, Offset = offset, Limit = pageSize });

        return new PagedResult<ContactReadModel>(items.ToList().AsReadOnly(), page, pageSize, totalCount);
    }
}
