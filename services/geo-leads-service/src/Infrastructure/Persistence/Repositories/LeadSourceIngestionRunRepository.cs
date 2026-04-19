using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GeoLeadsService.Infrastructure.Persistence.Repositories;

public sealed class LeadSourceIngestionRunRepository(GeoLeadsDbContext dbContext) : ILeadSourceIngestionRunRepository
{
    public async Task AddAsync(LeadSourceIngestionRun run, CancellationToken cancellationToken)
    {
        dbContext.LeadSourceIngestionRuns.Add(run);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(LeadSourceIngestionRun run, CancellationToken cancellationToken)
    {
        dbContext.LeadSourceIngestionRuns.Update(run);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LeadSourceIngestionRun>> ListRecentAsync(int limit, CancellationToken cancellationToken)
        => await dbContext.LeadSourceIngestionRuns
            .OrderByDescending(x => x.StartedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
}
