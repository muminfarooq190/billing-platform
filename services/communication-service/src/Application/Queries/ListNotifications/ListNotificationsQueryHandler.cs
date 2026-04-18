using CommunicationService.Api;
using CommunicationService.Application.Abstractions;
using Dapper;
using MediatR;
using System.Text;

namespace CommunicationService.Application.Queries.ListNotifications;

public sealed class ListNotificationsQueryHandler(IReadDbConnectionFactory connectionFactory, IFeatureGate featureGate, ITenantContext tenantContext) : IRequestHandler<ListNotificationsQuery, IReadOnlyList<NotificationListItemReadModel>>
{
    public async Task<IReadOnlyList<NotificationListItemReadModel>> Handle(ListNotificationsQuery request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.CommunicationLogsRead, request.TenantId, tenantContext.UserId, cancellationToken);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var sql = new StringBuilder(@"SELECT id, tenant_id AS TenantId, recipient_id AS RecipientId, recipient_type AS RecipientType, channel, subject, priority, status,
                                            reference_id AS ReferenceId, correlation_id AS CorrelationId, workflow_type AS WorkflowType, idempotency_key AS IdempotencyKey,
                                            retry_count AS RetryCount, last_error AS LastError, provider_message_id AS ProviderMessageId,
                                            sent_at AS SentAt, delivered_at AS DeliveredAt, read_at AS ReadAt, created_at AS CreatedAt
                                     FROM notifications WHERE tenant_id = @TenantId");

        if (!string.IsNullOrWhiteSpace(request.Status)) sql.Append(" AND status = @Status");
        if (!string.IsNullOrWhiteSpace(request.Channel)) sql.Append(" AND channel = @Channel");
        if (!string.IsNullOrWhiteSpace(request.ReferenceId)) sql.Append(" AND reference_id = @ReferenceId");
        if (!string.IsNullOrWhiteSpace(request.CorrelationId)) sql.Append(" AND correlation_id = @CorrelationId");
        if (!string.IsNullOrWhiteSpace(request.WorkflowType)) sql.Append(" AND workflow_type = @WorkflowType");
        if (request.RecipientId.HasValue) sql.Append(" AND recipient_id = @RecipientId");

        sql.Append(" ORDER BY created_at DESC OFFSET @Offset LIMIT @Limit");

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        var results = await dbConnection.QueryAsync<NotificationListItemReadModel>(
            sql.ToString(),
            new
            {
                request.TenantId,
                request.Status,
                request.Channel,
                request.ReferenceId,
                request.CorrelationId,
                request.WorkflowType,
                request.RecipientId,
                Offset = (page - 1) * pageSize,
                Limit = pageSize
            });

        return results.ToList().AsReadOnly();
    }
}
