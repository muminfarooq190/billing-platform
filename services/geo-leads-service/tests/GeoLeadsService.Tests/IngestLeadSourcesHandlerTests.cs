using FluentAssertions;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Application.Commands.IngestLeadSources;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using Xunit;

namespace GeoLeadsService.Tests;

public sealed class IngestLeadSourcesHandlerTests
{
    [Fact]
    public async Task IngestLeadSourcesCommandHandler_ShouldPersistFetchedRecords()
    {
        var repository = new StubLeadSourceRecordRepository();
        var runRepository = new StubLeadSourceIngestionRunRepository();
        var handler = new IngestLeadSourcesCommandHandler(
            [new StubAdapter()],
            repository,
            runRepository,
            new FakeTenantContext(),
            new AllowFeatureGate());

        var count = await handler.Handle(new IngestLeadSourcesCommand(), CancellationToken.None);

        count.Should().Be(1);
        repository.Stored.Should().ContainSingle();
        repository.Stored[0].RawName.Should().Be("Sample Lead");
        runRepository.Stored.Should().ContainSingle();
        runRepository.Stored[0].Status.Should().Be("Completed");
    }

    [Fact]
    public async Task IngestLeadSourcesCommandHandler_ShouldCombineMultipleAdapters()
    {
        var repository = new StubLeadSourceRecordRepository();
        var runRepository = new StubLeadSourceIngestionRunRepository();
        var handler = new IngestLeadSourcesCommandHandler(
            [new StubAdapter(), new SecondStubAdapter()],
            repository,
            runRepository,
            new FakeTenantContext(),
            new AllowFeatureGate());

        var count = await handler.Handle(new IngestLeadSourcesCommand(), CancellationToken.None);

        count.Should().Be(2);
        repository.Stored.Should().HaveCount(2);
        repository.Stored.Select(x => x.SourceName).Should().Contain(["stub-source", "second-stub-source"]);
        runRepository.Stored.Should().HaveCount(2);
    }

    [Fact]
    public async Task IngestLeadSourcesCommandHandler_ShouldSkipDisabledAdapters()
    {
        var repository = new StubLeadSourceRecordRepository();
        var runRepository = new StubLeadSourceIngestionRunRepository();
        var handler = new IngestLeadSourcesCommandHandler([new DisabledStubAdapter(), new StubAdapter()], repository, runRepository, new FakeTenantContext(), new AllowFeatureGate());

        var count = await handler.Handle(new IngestLeadSourcesCommand(), CancellationToken.None);

        count.Should().Be(1);
        repository.Stored.Should().ContainSingle();
        runRepository.Stored.Should().ContainSingle();
        runRepository.Stored[0].SourceName.Should().Be("stub-source");
    }

    [Fact]
    public async Task IngestLeadSourcesCommandHandler_ShouldUpsertDuplicateSourceRecords()
    {
        var repository = new StubLeadSourceRecordRepository();
        var runRepository = new StubLeadSourceIngestionRunRepository();
        repository.Stored.Add(new LeadSourceRecord("stub-source", "source-1", "Old Lead", "hotel", "Old", null, null, null, 18.90m, 72.80m, "{\"old\":true}"));
        var handler = new IngestLeadSourcesCommandHandler([new StubAdapter()], repository, runRepository, new FakeTenantContext(), new AllowFeatureGate());

        var count = await handler.Handle(new IngestLeadSourcesCommand(), CancellationToken.None);

        count.Should().Be(1);
        repository.Stored.Should().HaveCount(1);
        repository.Stored[0].RawName.Should().Be("Sample Lead");
        repository.Stored[0].RawCategory.Should().Be("tour_operator");
    }

    private sealed class FakeTenantContext : GeoLeadsService.Api.ITenantContext
    {
        public Guid TenantId { get; } = Guid.NewGuid();
    }

    private sealed class AllowFeatureGate : IFeatureGate
    {
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
    }

    private sealed class StubAdapter : IConfigurableGeoLeadSourceAdapter
    {
        public string SourceName => "stub-source";
        public bool IsEnabled => true;

        public Task<IReadOnlyList<GeoLeadSourceRecordInput>> FetchAsync(CancellationToken cancellationToken, GeoBoundingBox? boundingBox = null)
            => Task.FromResult<IReadOnlyList<GeoLeadSourceRecordInput>>(
            [
                new("source-1", "Sample Lead", "tour_operator", "Somewhere", null, "hello@example.com", null, 18.92m, 72.83m, "{}")
            ]);
    }

    private sealed class SecondStubAdapter : IConfigurableGeoLeadSourceAdapter
    {
        public string SourceName => "second-stub-source";
        public bool IsEnabled => true;

        public Task<IReadOnlyList<GeoLeadSourceRecordInput>> FetchAsync(CancellationToken cancellationToken, GeoBoundingBox? boundingBox = null)
            => Task.FromResult<IReadOnlyList<GeoLeadSourceRecordInput>>(
            [
                new("source-2", "Second Lead", "hotel", "Elsewhere", null, "stay@example.com", null, 18.94m, 72.84m, "{}")
            ]);
    }

    private sealed class DisabledStubAdapter : IConfigurableGeoLeadSourceAdapter
    {
        public string SourceName => "disabled-source";
        public bool IsEnabled => false;

        public Task<IReadOnlyList<GeoLeadSourceRecordInput>> FetchAsync(CancellationToken cancellationToken, GeoBoundingBox? boundingBox = null)
            => Task.FromResult<IReadOnlyList<GeoLeadSourceRecordInput>>(
            [
                new("disabled-1", "Disabled Lead", "hotel", "Nowhere", null, null, null, null, null, "{}")
            ]);
    }

    private sealed class StubLeadSourceRecordRepository : ILeadSourceRecordRepository
    {
        public List<LeadSourceRecord> Stored { get; } = [];

        public Task AddRangeAsync(IReadOnlyCollection<LeadSourceRecord> records, CancellationToken cancellationToken)
        {
            Stored.AddRange(records);
            return Task.CompletedTask;
        }

        public Task UpsertRangeAsync(IReadOnlyCollection<LeadSourceRecord> records, CancellationToken cancellationToken)
        {
            foreach (var record in records)
            {
                var existing = Stored.SingleOrDefault(x => x.SourceName == record.SourceName && x.SourceRecordId == record.SourceRecordId);
                if (existing is null)
                {
                    Stored.Add(record);
                    continue;
                }

                existing.Refresh(
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

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<LeadSourceRecord>> ListAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<LeadSourceRecord>>(Stored);

        public Task<IReadOnlyList<LeadSourceRecord>> ListRecentAsync(int limit, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<LeadSourceRecord>>(Stored.OrderByDescending(x => x.LastSeenAt).Take(limit).ToList());
    }

    private sealed class StubLeadSourceIngestionRunRepository : ILeadSourceIngestionRunRepository
    {
        public List<LeadSourceIngestionRun> Stored { get; } = [];

        public Task AddAsync(LeadSourceIngestionRun run, CancellationToken cancellationToken)
        {
            Stored.Add(run);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(LeadSourceIngestionRun run, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task<IReadOnlyList<LeadSourceIngestionRun>> ListRecentAsync(int limit, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<LeadSourceIngestionRun>>(Stored.OrderByDescending(x => x.StartedAt).Take(limit).ToList());
    }
}
