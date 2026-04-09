using Microsoft.EntityFrameworkCore;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class EntityNoteRepository(TravelDbContext dbContext) : IEntityNoteRepository
{
    public Task AddAsync(EntityNote note, CancellationToken cancellationToken)
        => dbContext.EntityNotes.AddAsync(note, cancellationToken).AsTask();

    public Task<EntityNote?> GetByIdAsync(Guid noteId, CancellationToken cancellationToken)
        => dbContext.EntityNotes.SingleOrDefaultAsync(x => x.Id == noteId, cancellationToken);

    public async Task<IReadOnlyList<EntityNote>> ListByEntityAsync(Guid tenantId, string entityType, Guid entityId, CancellationToken cancellationToken)
        => await dbContext.EntityNotes
            .Where(x => x.TenantId == tenantId && x.EntityType == entityType && x.EntityId == entityId && x.DeletedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task UpdateAsync(EntityNote note, CancellationToken cancellationToken)
    {
        dbContext.EntityNotes.Update(note);
        return Task.CompletedTask;
    }
}
