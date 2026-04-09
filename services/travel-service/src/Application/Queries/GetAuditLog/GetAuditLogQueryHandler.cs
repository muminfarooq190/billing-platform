using Dapper;
using MediatR;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Queries.GetAuditLog;

public sealed class GetAuditLogQueryHandler(IReadDbConnectionFactory connectionFactory, IFeatureGate featureGate) : IRequestHandler<GetAuditLogQuery, AuditLogPageReadModel>
{
    public async Task<AuditLogPageReadModel> Handle(GetAuditLogQuery request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.TravelAuditRead, request.TenantId, cancellationToken);

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);
        var offset = (page - 1) * pageSize;

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;

        var totalCount = await dbConnection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*)
               FROM audit_logs
               WHERE tenant_id = @TenantId AND entity_type = @EntityType AND entity_id = @EntityId",
            new { request.TenantId, request.EntityType, request.EntityId });

        var items = await dbConnection.QueryAsync<AuditLogReadModel>(
            @"SELECT id,
                      entity_type AS EntityType,
                      entity_id AS EntityId,
                      action,
                      actor_user_id AS ActorUserId,
                      ip_address AS IpAddress,
                      user_agent AS UserAgent,
                      before_json AS BeforeJson,
                      after_json AS AfterJson,
                      metadata_json AS MetadataJson,
                      occurred_at AS OccurredAt
               FROM audit_logs
               WHERE tenant_id = @TenantId AND entity_type = @EntityType AND entity_id = @EntityId
               ORDER BY occurred_at DESC
               LIMIT @PageSize OFFSET @Offset",
            new { request.TenantId, request.EntityType, request.EntityId, PageSize = pageSize, Offset = offset });

        return new AuditLogPageReadModel(items.ToList(), page, pageSize, totalCount);
    }
}
