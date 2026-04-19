using GeoLeadsService.Domain.Aggregates;

namespace GeoLeadsService.Domain.Repositories;

public interface ILeadSourceIngestionRunRepository
{
    Task AddAsync(LeadSourceIngestionRun run, CancellationToken cancellationToken);
    Task UpdateAsync(LeadSourceIngestionRun run, CancellationToken cancellationToken);
    Task<IReadOnlyList<LeadSourceIngestionRun>> ListRecentAsync(int limit, CancellationToken cancellationToken);
}
