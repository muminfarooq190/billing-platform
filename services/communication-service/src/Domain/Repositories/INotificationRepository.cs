using CommunicationService.Domain.Aggregates;
using CommunicationService.Domain.Enums;

namespace CommunicationService.Domain.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Notification?> GetByIdempotencyKeyAsync(Guid tenantId, string idempotencyKey, CancellationToken cancellationToken);

    /// <summary>
    /// Look up a notification by the provider's external message id (Twilio
    /// SID, SendGrid X-Message-Id, etc). Used by inbound status webhooks to
    /// correlate a delivery callback back to our aggregate.
    /// </summary>
    Task<Notification?> GetByProviderMessageIdAsync(string providerMessageId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Notification>> ListByRecipientIdAsync(Guid recipientId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Notification>> ListPendingAsync(int batchSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<Notification>> ListRetryableAsync(int maxRetries, int batchSize, CancellationToken cancellationToken);
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken);
}
