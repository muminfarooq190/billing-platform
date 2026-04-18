using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using MediatR;

namespace GeoLeadsService.Application.Commands.IngestLeadSources;

public sealed class IngestLeadSourcesCommandHandler(
    IEnumerable<IGeoLeadSourceAdapter> geoLeadSourceAdapters,
    ILeadSourceRecordRepository leadSourceRecordRepository) : IRequestHandler<IngestLeadSourcesCommand, int>
{
    public async Task<int> Handle(IngestLeadSourcesCommand request, CancellationToken cancellationToken)
    {
        var records = new List<LeadSourceRecord>();
        foreach (var adapter in geoLeadSourceAdapters)
        {
            var fetched = await adapter.FetchAsync(cancellationToken);
            records.AddRange(fetched.Select(x => new LeadSourceRecord(
                adapter.SourceName,
                x.SourceRecordId,
                x.RawName,
                x.RawCategory,
                x.RawAddress,
                x.RawPhone,
                x.RawEmail,
                x.RawWebsite,
                x.RawLatitude,
                x.RawLongitude,
                x.RawPayloadJson)));
        }

        if (records.Count > 0)
            await leadSourceRecordRepository.AddRangeAsync(records, cancellationToken);

        return records.Count;
    }
}
