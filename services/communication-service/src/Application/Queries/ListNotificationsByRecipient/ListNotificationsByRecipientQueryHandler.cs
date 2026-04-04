using Dapper;
using CommunicationService.Application.Abstractions;
using MediatR;

namespace CommunicationService.Application.Queries.ListNotificationsByRecipient;

public sealed class ListNotificationsByRecipientQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<ListNotificationsByRecipientQuery, IReadOnlyList<NotificationReadModel>>
{
    public async Task<IReadOnlyList<NotificationReadModel>> Handle(ListNotificationsByRecipientQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        var results = await dbConnection.QueryAsync<NotificationReadModel>(
            "SELECT id, tenant_id AS TenantId, recipient_id AS RecipientId, recipient_type AS RecipientType, channel, subject, priority, status, retry_count AS RetryCount, last_error AS LastError, provider_message_id AS ProviderMessageId, sent_at AS SentAt, delivered_at AS DeliveredAt, created_at AS CreatedAt FROM notifications WHERE recipient_id = @RecipientId ORDER BY created_at DESC LIMIT 50",
            new { request.RecipientId });
        return results.ToList().AsReadOnly();
    }
}
