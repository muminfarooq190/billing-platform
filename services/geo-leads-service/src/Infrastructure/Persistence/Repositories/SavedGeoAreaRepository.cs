using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GeoLeadsService.Infrastructure.Persistence.Repositories;

public sealed class SavedGeoAreaRepository(GeoLeadsDbContext dbContext) : ISavedGeoAreaRepository
{
    public async Task AddAsync(SavedGeoArea area, CancellationToken cancellationToken)
    {
        dbContext.SavedGeoAreas.Add(area);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(SavedGeoArea area, CancellationToken cancellationToken)
    {
        dbContext.SavedGeoAreas.Update(area);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(SavedGeoArea area, CancellationToken cancellationToken)
    {
        dbContext.SavedGeoAreas.Remove(area);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<SavedGeoArea?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken)
        => dbContext.SavedGeoAreas.SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);

    public async Task<IReadOnlyList<SavedGeoArea>> ListByTenantAsync(Guid tenantId, int limit, CancellationToken cancellationToken)
        => await dbContext.SavedGeoAreas
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
}
