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

    public async Task UpsertRangeAsync(IReadOnlyCollection<LeadSourceRecord> records, CancellationToken cancellationToken)
    {
        var keys = records.Select(x => new { x.SourceName, x.SourceRecordId }).Distinct().ToList();
        var sourceNames = keys.Select(x => x.SourceName).Distinct().ToList();
        var sourceRecordIds = keys.Select(x => x.SourceRecordId).Distinct().ToList();

        var existing = await dbContext.Set<LeadSourceRecord>()
            .Where(x => sourceNames.Contains(x.SourceName) && sourceRecordIds.Contains(x.SourceRecordId))
            .ToListAsync(cancellationToken);

        foreach (var record in records)
        {
            var match = existing.SingleOrDefault(x => x.SourceName == record.SourceName && x.SourceRecordId == record.SourceRecordId);
            if (match is null)
            {
                dbContext.Set<LeadSourceRecord>().Add(record);
                continue;
            }

            match.Refresh(
                record.RawName,
                record.RawCategory,
                record.RawAddress,
                record.RawPhone,
                record.RawEmail,
                record.RawWebsite,
                record.RawLatitude,
                record.RawLongitude,
                record.RawPayloadJson);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<LeadSourceRecord>> ListAsync(CancellationToken cancellationToken)
        => dbContext.Set<LeadSourceRecord>().OrderByDescending(x => x.LastSeenAt).ToListAsync(cancellationToken).ContinueWith(t => (IReadOnlyList<LeadSourceRecord>)t.Result, cancellationToken);

    public async Task<IReadOnlyList<LeadSourceRecord>> ListRecentAsync(int limit, CancellationToken cancellationToken)
        => await dbContext.Set<LeadSourceRecord>()
            .OrderByDescending(x => x.LastSeenAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
}
