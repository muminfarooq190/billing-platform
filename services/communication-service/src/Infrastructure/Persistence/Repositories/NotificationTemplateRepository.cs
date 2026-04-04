using CommunicationService.Domain.Aggregates;
using CommunicationService.Domain.Enums;
using CommunicationService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CommunicationService.Infrastructure.Persistence.Repositories;

public sealed class NotificationTemplateRepository(CommunicationDbContext dbContext) : INotificationTemplateRepository
{
    public Task AddAsync(NotificationTemplate template, CancellationToken cancellationToken) => dbContext.NotificationTemplates.AddAsync(template, cancellationToken).AsTask();

    public Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => dbContext.NotificationTemplates.SingleOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

    public Task<NotificationTemplate?> GetByNameAndTenantAsync(string name, Guid tenantId, CancellationToken cancellationToken) => dbContext.NotificationTemplates.SingleOrDefaultAsync(x => x.Name == name && x.TenantId == tenantId && x.DeletedAt == null, cancellationToken);

    public async Task<IReadOnlyList<NotificationTemplate>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => await dbContext.NotificationTemplates.Where(x => x.TenantId == tenantId && x.DeletedAt == null).OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task UpdateAsync(NotificationTemplate template, CancellationToken cancellationToken)
    {
        dbContext.NotificationTemplates.Update(template);
        return Task.CompletedTask;
    }
}
