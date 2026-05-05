using Microsoft.EntityFrameworkCore;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class TravelTemplateRepository(TravelDbContext dbContext) : ITravelTemplateRepository
{
    public Task AddAsync(TravelTemplate template, CancellationToken cancellationToken)
        => dbContext.Set<TravelTemplate>().AddAsync(template, cancellationToken).AsTask();

    public Task<TravelTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Set<TravelTemplate>().SingleOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

    public async Task<IReadOnlyList<TravelTemplate>> ListByTenantAsync(Guid tenantId, TravelTemplateContext? context, CancellationToken cancellationToken)
    {
        var query = dbContext.Set<TravelTemplate>()
            .Where(x => x.TenantId == tenantId && x.DeletedAt == null);

        if (context.HasValue)
            query = query.Where(x => x.Context == context.Value);

        return await query
            .OrderByDescending(x => x.IsBuiltIn)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(TravelTemplate template, CancellationToken cancellationToken)
    {
        dbContext.Set<TravelTemplate>().Update(template);
        return Task.CompletedTask;
    }
}
