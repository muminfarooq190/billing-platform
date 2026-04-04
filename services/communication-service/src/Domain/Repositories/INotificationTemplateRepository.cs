using CommunicationService.Domain.Aggregates;

namespace CommunicationService.Domain.Repositories;

public interface INotificationTemplateRepository
{
    Task AddAsync(NotificationTemplate template, CancellationToken cancellationToken);
    Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<NotificationTemplate?> GetByNameAndTenantAsync(string name, Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyList<NotificationTemplate>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken);
    Task UpdateAsync(NotificationTemplate template, CancellationToken cancellationToken);
}
