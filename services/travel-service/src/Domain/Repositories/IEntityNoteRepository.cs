using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface IEntityNoteRepository
{
    Task AddAsync(EntityNote note, CancellationToken cancellationToken);
    Task<EntityNote?> GetByIdAsync(Guid noteId, CancellationToken cancellationToken);
    Task<IReadOnlyList<EntityNote>> ListByEntityAsync(Guid tenantId, string entityType, Guid entityId, CancellationToken cancellationToken);
    Task UpdateAsync(EntityNote note, CancellationToken cancellationToken);
}
