using Dapper;
using MediatR;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Queries.GetWorkQueue;

public sealed class GetWorkQueueQueryHandler(IReadDbConnectionFactory connectionFactory, IFeatureGate featureGate) : IRequestHandler<GetWorkQueueQuery, IReadOnlyList<WorkQueueItemReadModel>>
{
    public async Task<IReadOnlyList<WorkQueueItemReadModel>> Handle(GetWorkQueueQuery request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.TravelTimelineRead, request.TenantId, cancellationToken);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var items = await connection.QueryAsync<WorkQueueItemReadModel>(@"
            SELECT id,
                   'FollowUp' AS WorkType,
                   subject,
                   priority,
                   status,
                   due_date AS DueDate,
                   assigned_to_user_id AS AssignedToUserId,
                   CASE
                     WHEN status = 'Completed' THEN 'Done'
                     WHEN due_date IS NOT NULL AND due_date < NOW() THEN 'Overdue'
                     WHEN due_date IS NOT NULL AND due_date < NOW() + INTERVAL '24 hours' THEN 'DueSoon'
                     ELSE 'Open'
                   END AS QueueState
            FROM follow_ups
            WHERE tenant_id = @TenantId AND deleted_at IS NULL
            ORDER BY due_date NULLS LAST, created_at DESC
            OFFSET @Offset LIMIT @Limit", new
        {
            request.TenantId,
            Offset = (page - 1) * pageSize,
            Limit = pageSize
        });

        return items.ToList();
    }
}
