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
        var handler = new IngestLeadSourcesCommandHandler(
            [new StubAdapter()],
            repository);

        var count = await handler.Handle(new IngestLeadSourcesCommand(), CancellationToken.None);

        count.Should().Be(1);
        repository.Stored.Should().ContainSingle();
        repository.Stored[0].RawName.Should().Be("Sample Lead");
    }

    private sealed class StubAdapter : IGeoLeadSourceAdapter
    {
        public string SourceName => "stub-source";

        public Task<IReadOnlyList<GeoLeadSourceRecordInput>> FetchAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<GeoLeadSourceRecordInput>>(
            [
                new("source-1", "Sample Lead", "tour_operator", "Somewhere", null, "hello@example.com", null, 18.92m, 72.83m, "{}")
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

        public Task<IReadOnlyList<LeadSourceRecord>> ListAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<LeadSourceRecord>>(Stored);
    }
}
