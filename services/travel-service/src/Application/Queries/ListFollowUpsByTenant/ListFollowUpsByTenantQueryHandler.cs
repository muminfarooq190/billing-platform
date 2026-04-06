using Dapper;
using TravelService.Application.Abstractions;
using TravelService.Application.Queries.GetFollowUpById;
using MediatR;
using System.Text;

namespace TravelService.Application.Queries.ListFollowUpsByTenant;

public sealed class ListFollowUpsByTenantQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<ListFollowUpsByTenantQuery, IReadOnlyList<FollowUpReadModel>>
{
    public async Task<IReadOnlyList<FollowUpReadModel>> Handle(ListFollowUpsByTenantQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;

        var sql = new StringBuilder("SELECT id, tenant_id AS TenantId, customer_contact_id AS CustomerContactId, customer_name AS CustomerName, subject, notes, priority, status, due_date AS DueDate, assigned_to_user_id AS AssignedToUserId, completed_at AS CompletedAt, created_at AS CreatedAt, updated_at AS UpdatedAt FROM follow_ups WHERE tenant_id = @TenantId AND deleted_at IS NULL");

        if (!string.IsNullOrWhiteSpace(request.Status))
            sql.Append(" AND status = @Status");
        if (!string.IsNullOrWhiteSpace(request.CustomerName))
            sql.Append(" AND customer_name ILIKE @CustomerName");
        if (request.DueDateFrom.HasValue)
            sql.Append(" AND due_date >= @DueDateFrom");
        if (request.DueDateTo.HasValue)
            sql.Append(" AND due_date <= @DueDateTo");

        sql.Append(" ORDER BY due_date OFFSET @Offset LIMIT @Limit");

        var results = await dbConnection.QueryAsync<FollowUpReadModel>(sql.ToString(), new
        {
            request.TenantId,
            request.Status,
            CustomerName = string.IsNullOrWhiteSpace(request.CustomerName) ? null : $"%{request.CustomerName.Trim()}%",
            request.DueDateFrom,
            request.DueDateTo,
            Offset = (page - 1) * pageSize,
            Limit = pageSize
        });

        return results.ToList().AsReadOnly();
    }
}
