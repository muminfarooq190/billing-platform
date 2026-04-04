using Dapper;
using TravelService.Application.Abstractions;
using MediatR;

namespace TravelService.Application.Queries.GetFollowUpById;

public sealed class GetFollowUpByIdQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<GetFollowUpByIdQuery, FollowUpReadModel?>
{
    public async Task<FollowUpReadModel?> Handle(GetFollowUpByIdQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        return await dbConnection.QuerySingleOrDefaultAsync<FollowUpReadModel>(
            "SELECT id, tenant_id AS TenantId, customer_contact_id AS CustomerContactId, customer_name AS CustomerName, subject, notes, priority, status, due_date AS DueDate, assigned_to_user_id AS AssignedToUserId, completed_at AS CompletedAt, created_at AS CreatedAt, updated_at AS UpdatedAt FROM follow_ups WHERE id = @Id AND deleted_at IS NULL",
            new { request.Id });
    }
}
