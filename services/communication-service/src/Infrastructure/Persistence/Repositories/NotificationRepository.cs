using CommunicationService.Domain.Aggregates;
using CommunicationService.Domain.Enums;
using CommunicationService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CommunicationService.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository(CommunicationDbContext dbContext) : INotificationRepository
{
    public Task AddAsync(Notification notification, CancellationToken cancellationToken) => dbContext.Notifications.AddAsync(notification, cancellationToken).AsTask();

    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => dbContext.Notifications.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Notification>> ListByRecipientIdAsync(Guid recipientId, CancellationToken cancellationToken) => await dbContext.Notifications.Where(x => x.RecipientId == recipientId).OrderByDescending(x => x.CreatedAt).Take(50).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Notification>> ListPendingAsync(int batchSize, CancellationToken cancellationToken) => await dbContext.Notifications.Where(x => x.Status == NotificationStatus.Queued).OrderBy(x => x.Priority).ThenBy(x => x.CreatedAt).Take(batchSize).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Notification>> ListRetryableAsync(int maxRetries, int batchSize, CancellationToken cancellationToken) => await dbContext.Notifications.Where(x => x.Status == NotificationStatus.Failed && x.RetryCount < maxRetries).OrderBy(x => x.CreatedAt).Take(batchSize).ToListAsync(cancellationToken);

    public Task UpdateAsync(Notification notification, CancellationToken cancellationToken)
    {
        dbContext.Notifications.Update(notification);
        return Task.CompletedTask;
    }
}
