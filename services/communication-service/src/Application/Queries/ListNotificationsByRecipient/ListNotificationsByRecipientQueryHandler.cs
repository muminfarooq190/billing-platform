using Dapper;
using CommunicationService.Api;
using CommunicationService.Application.Abstractions;
using MediatR;

namespace CommunicationService.Application.Queries.ListNotificationsByRecipient;

public sealed class ListNotificationsByRecipientQueryHandler(IReadDbConnectionFactory connectionFactory, IFeatureGate featureGate, ITenantContext tenantContext) : IRequestHandler<ListNotificationsByRecipientQuery, IReadOnlyList<NotificationReadModel>>
{
    public async Task<IReadOnlyList<NotificationReadModel>> Handle(ListNotificationsByRecipientQuery request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.CommunicationLogsRead, request.TenantId, tenantContext.UserId, cancellationToken);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        var results = await dbConnection.QueryAsync<NotificationReadModel>(
            "SELECT id, tenant_id AS TenantId, recipient_id AS RecipientId, recipient_type AS RecipientType, channel, subject, priority, status, retry_count AS RetryCount, last_error AS LastError, provider_message_id AS ProviderMessageId, sent_at AS SentAt, delivered_at AS DeliveredAt, read_at AS ReadAt, created_at AS CreatedAt FROM notifications WHERE tenant_id = @TenantId AND recipient_id = @RecipientId ORDER BY created_at DESC OFFSET @Offset LIMIT @Limit",
            new { request.TenantId, request.RecipientId, Offset = (page - 1) * pageSize, Limit = pageSize });
        return results.ToList().AsReadOnly();
    }
}
