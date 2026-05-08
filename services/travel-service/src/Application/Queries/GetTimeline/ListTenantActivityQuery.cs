using Dapper;
using MediatR;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Queries.GetTimeline;

public sealed record ListTenantActivityQuery(
    Guid TenantId,
    int Page = 1,
    int PageSize = 20,
    string? EntityType = null,
    string? ActivityType = null,
    Guid? ActorUserId = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null) : IRequest<TimelinePageReadModel>;

public sealed class ListTenantActivityQueryHandler(IReadDbConnectionFactory connectionFactory, IFeatureGate featureGate) : IRequestHandler<ListTenantActivityQuery, TimelinePageReadModel>
{
    public async Task<TimelinePageReadModel> Handle(ListTenantActivityQuery request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.TravelTimelineRead, request.TenantId, cancellationToken);

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);
        var offset = (page - 1) * pageSize;

        var entityType = string.IsNullOrWhiteSpace(request.EntityType) ? null : request.EntityType.Trim();
        var activityType = string.IsNullOrWhiteSpace(request.ActivityType) ? null : request.ActivityType.Trim();

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;

        var parameters = new
        {
            request.TenantId,
            EntityType = entityType,
            ActivityType = activityType,
            ActorUserId = request.ActorUserId,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            PageSize = pageSize,
            Offset = offset,
        };

        const string filterClause = @"
            WHERE tenant_id = @TenantId
              AND (@EntityType IS NULL OR entity_type = @EntityType)
              AND (@ActivityType IS NULL OR activity_type = @ActivityType)
              AND (@ActorUserId IS NULL OR actor_user_id = @ActorUserId)
              AND (@FromDate IS NULL OR occurred_at >= @FromDate)
              AND (@ToDate IS NULL OR occurred_at <= @ToDate)";

        var totalCount = await dbConnection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM activity_entries " + filterClause,
            parameters);

        var items = await dbConnection.QueryAsync<ActivityEntryReadModel>(
            @"SELECT id,
                      entity_type AS EntityType,
                      entity_id AS EntityId,
                      activity_type AS ActivityType,
                      summary,
                      detail_json AS DetailJson,
                      actor_user_id AS ActorUserId,
                      occurred_at AS OccurredAt
               FROM activity_entries " + filterClause + @"
               ORDER BY occurred_at DESC
               LIMIT @PageSize OFFSET @Offset",
            parameters);

        return new TimelinePageReadModel(items.ToList(), page, pageSize, totalCount);
    }
}
