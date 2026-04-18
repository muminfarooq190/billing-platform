using CommunicationService.Api;
using CommunicationService.Application.Abstractions;
using Dapper;
using MediatR;

namespace CommunicationService.Application.Queries.GetNotificationDetail;

public sealed class GetNotificationDetailQueryHandler(IReadDbConnectionFactory connectionFactory, IFeatureGate featureGate, ITenantContext tenantContext) : IRequestHandler<GetNotificationDetailQuery, NotificationDetailReadModel?>
{
    public async Task<NotificationDetailReadModel?> Handle(GetNotificationDetailQuery request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.CommunicationLogsRead, request.TenantId, tenantContext.UserId, cancellationToken);

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        return await dbConnection.QuerySingleOrDefaultAsync<NotificationDetailReadModel>(
            @"SELECT id, tenant_id AS TenantId, recipient_id AS RecipientId, recipient_type AS RecipientType, channel, subject, body, priority, status,
                      reference_id AS ReferenceId, correlation_id AS CorrelationId, workflow_type AS WorkflowType, idempotency_key AS IdempotencyKey,
                      document_references_json AS DocumentReferencesJson, metadata_json AS MetadataJson, retry_count AS RetryCount, last_error AS LastError,
                      provider_message_id AS ProviderMessageId, sent_at AS SentAt, delivered_at AS DeliveredAt, read_at AS ReadAt, created_at AS CreatedAt, updated_at AS UpdatedAt
               FROM notifications WHERE tenant_id = @TenantId AND id = @NotificationId",
            new { request.TenantId, request.NotificationId });
    }
}
