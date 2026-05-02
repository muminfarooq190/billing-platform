using Dapper;
using MediatR;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Queries.GetTimeline;

public sealed record ListTenantActivityQuery(Guid TenantId, int Page = 1, int PageSize = 20) : IRequest<TimelinePageReadModel>;

public sealed class ListTenantActivityQueryHandler(IReadDbConnectionFactory connectionFactory, IFeatureGate featureGate) : IRequestHandler<ListTenantActivityQuery, TimelinePageReadModel>
{
    public async Task<TimelinePageReadModel> Handle(ListTenantActivityQuery request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.TravelTimelineRead, request.TenantId, cancellationToken);

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);
        var offset = (page - 1) * pageSize;

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;

        var totalCount = await dbConnection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*)
               FROM activity_entries
               WHERE tenant_id = @TenantId",
            new { request.TenantId });

        var items = await dbConnection.QueryAsync<ActivityEntryReadModel>(
            @"SELECT id,
                      entity_type AS EntityType,
                      entity_id AS EntityId,
                      activity_type AS ActivityType,
                      summary,
                      detail_json AS DetailJson,
                      actor_user_id AS ActorUserId,
                      occurred_at AS OccurredAt
               FROM activity_entries
               WHERE tenant_id = @TenantId
               ORDER BY occurred_at DESC
               LIMIT @PageSize OFFSET @Offset",
            new { request.TenantId, PageSize = pageSize, Offset = offset });

        return new TimelinePageReadModel(items.ToList(), page, pageSize, totalCount);
    }
}
