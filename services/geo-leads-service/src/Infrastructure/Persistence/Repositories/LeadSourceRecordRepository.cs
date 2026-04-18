using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GeoLeadsService.Infrastructure.Persistence.Repositories;

public sealed class LeadSourceRecordRepository(GeoLeadsDbContext dbContext) : ILeadSourceRecordRepository
{
    public async Task AddRangeAsync(IReadOnlyCollection<LeadSourceRecord> records, CancellationToken cancellationToken)
    {
        dbContext.Set<LeadSourceRecord>().AddRange(records);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<LeadSourceRecord>> ListAsync(CancellationToken cancellationToken)
        => dbContext.Set<LeadSourceRecord>().OrderByDescending(x => x.LastSeenAt).ToListAsync(cancellationToken).ContinueWith(t => (IReadOnlyList<LeadSourceRecord>)t.Result, cancellationToken);
}
