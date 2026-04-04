using Dapper;
using TravelService.Application.Abstractions;
using TravelService.Application.Queries.GetFollowUpById;
using MediatR;

namespace TravelService.Application.Queries.ListFollowUpsByTenant;

public sealed class ListFollowUpsByTenantQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<ListFollowUpsByTenantQuery, IReadOnlyList<FollowUpReadModel>>
{
    public async Task<IReadOnlyList<FollowUpReadModel>> Handle(ListFollowUpsByTenantQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        var results = await dbConnection.QueryAsync<FollowUpReadModel>(
            "SELECT id, tenant_id AS TenantId, customer_contact_id AS CustomerContactId, customer_name AS CustomerName, subject, notes, priority, status, due_date AS DueDate, assigned_to_user_id AS AssignedToUserId, completed_at AS CompletedAt, created_at AS CreatedAt, updated_at AS UpdatedAt FROM follow_ups WHERE tenant_id = @TenantId AND deleted_at IS NULL ORDER BY due_date",
            new { request.TenantId });
        return results.ToList().AsReadOnly();
    }
}
