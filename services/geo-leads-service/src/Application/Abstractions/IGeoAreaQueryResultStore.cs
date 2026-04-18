using GeoLeadsService.Domain.Aggregates;

namespace GeoLeadsService.Application.Abstractions;

public interface IGeoAreaQueryResultStore
{
    Task SaveAsync(Guid queryId, IReadOnlyList<GeoAreaQueryResult> results, CancellationToken cancellationToken);
    Task<IReadOnlyList<GeoAreaQueryResult>> GetAsync(Guid queryId, CancellationToken cancellationToken);
}
