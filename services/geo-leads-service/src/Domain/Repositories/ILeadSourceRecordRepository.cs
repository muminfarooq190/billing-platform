using GeoLeadsService.Domain.Aggregates;

namespace GeoLeadsService.Domain.Repositories;

public interface ILeadSourceRecordRepository
{
    Task AddRangeAsync(IReadOnlyCollection<LeadSourceRecord> records, CancellationToken cancellationToken);
    Task UpsertRangeAsync(IReadOnlyCollection<LeadSourceRecord> records, CancellationToken cancellationToken);
    Task<IReadOnlyList<LeadSourceRecord>> ListAsync(CancellationToken cancellationToken);
}
