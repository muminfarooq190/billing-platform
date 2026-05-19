using GeoLeadsService.Api;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using MediatR;

namespace GeoLeadsService.Application.Commands.IngestLeadSources;

public sealed class IngestLeadSourcesCommandHandler(
    IEnumerable<IGeoLeadSourceAdapter> geoLeadSourceAdapters,
    ILeadSourceRecordRepository leadSourceRecordRepository,
    ILeadSourceIngestionRunRepository leadSourceIngestionRunRepository,
    ITenantContext tenantContext,
    IFeatureGate featureGate) : IRequestHandler<IngestLeadSourcesCommand, int>
{
    public async Task<int> Handle(IngestLeadSourcesCommand request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync("geo-leads.manage", tenantContext.TenantId, cancellationToken);
        var total = 0;

        foreach (var adapter in geoLeadSourceAdapters.Where(x => x is not IConfigurableGeoLeadSourceAdapter configurable || configurable.IsEnabled))
        {
            var run = new LeadSourceIngestionRun(adapter.SourceName);
            await leadSourceIngestionRunRepository.AddAsync(run, cancellationToken);

            try
            {
                var fetched = await adapter.FetchAsync(cancellationToken, request.BoundingBox);
                var records = fetched.Select(x => new LeadSourceRecord(
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
                    x.RawPayloadJson)).ToList();

                if (records.Count > 0)
                    await leadSourceRecordRepository.UpsertRangeAsync(records, cancellationToken);

                run.Complete(records.Count);
                await leadSourceIngestionRunRepository.UpdateAsync(run, cancellationToken);
                total += records.Count;
            }
            catch (Exception ex)
            {
                run.Fail(ex.Message);
                await leadSourceIngestionRunRepository.UpdateAsync(run, cancellationToken);
                throw;
            }
        }

        return total;
    }
}
