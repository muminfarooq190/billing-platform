using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GeoLeadsService.Infrastructure.Persistence.Repositories;

public sealed class GeoAreaQueryRepository(GeoLeadsDbContext dbContext) : IGeoAreaQueryRepository
{
    public async Task AddAsync(GeoAreaQuery query, CancellationToken cancellationToken)
    {
        dbContext.GeoAreaQueries.Add(query);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(GeoAreaQuery query, CancellationToken cancellationToken)
    {
        dbContext.GeoAreaQueries.Update(query);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<GeoAreaQuery?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken)
        => dbContext.GeoAreaQueries
            .Include(x => x.Results)
            .SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);

    public async Task<IReadOnlyList<GeoAreaQuery>> ListByTenantAsync(Guid tenantId, int limit, CancellationToken cancellationToken)
        => await dbContext.GeoAreaQueries
            .Include(x => x.Results)
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
}
