using CommunicationService.Domain.Aggregates;
using CommunicationService.Domain.Enums;

namespace CommunicationService.Domain.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Notification>> ListByRecipientIdAsync(Guid recipientId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Notification>> ListPendingAsync(int batchSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<Notification>> ListRetryableAsync(int maxRetries, int batchSize, CancellationToken cancellationToken);
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken);
}
