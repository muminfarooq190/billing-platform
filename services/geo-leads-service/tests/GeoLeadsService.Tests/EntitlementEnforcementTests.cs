using FluentAssertions;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Application.Commands.IngestLeadSources;
using GeoLeadsService.Application.Commands.SubmitGeoAreaQuery;
using GeoLeadsService.Application.Queries.GetGeoAreaQueryById;
using GeoLeadsService.Application.Queries.ListGeoAreaQueries;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Exceptions;
using GeoLeadsService.Domain.Repositories;
using Xunit;

namespace GeoLeadsService.Tests;

public sealed class EntitlementEnforcementTests
{
    [Fact]
    public async Task SubmitGeoAreaQuery_ShouldFail_WhenManageFeatureDisabled()
    {
        var handler = new SubmitGeoAreaQueryCommandHandler(new ThrowingGeoAreaQueryRepository(), new ThrowingGeoLeadCatalog(), new DenyFeatureGate());
        var polygon = new GeoPolygon([
            new GeoCoordinate(72.82m, 18.92m),
            new GeoCoordinate(72.84m, 18.92m),
            new GeoCoordinate(72.84m, 18.94m),
            new GeoCoordinate(72.82m, 18.94m),
            new GeoCoordinate(72.82m, 18.92m)
        ]);

        var act = async () => await handler.Handle(new SubmitGeoAreaQueryCommand(Guid.NewGuid(), polygon, ["hotel"], 10, "relevance"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*not enabled*");
    }

    [Fact]
    public async Task ListGeoAreaQueries_ShouldFail_WhenReadFeatureDisabled()
    {
        var handler = new ListGeoAreaQueriesQueryHandler(new ThrowingGeoAreaQueryRepository(), new DenyFeatureGate());

        var act = async () => await handler.Handle(new ListGeoAreaQueriesQuery(Guid.NewGuid(), 10), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*not enabled*");
    }

    [Fact]
    public async Task GetGeoAreaQueryById_ShouldFail_WhenReadFeatureDisabled()
    {
        var handler = new GetGeoAreaQueryByIdQueryHandler(new ThrowingGeoAreaQueryRepository(), new DenyFeatureGate());

        var act = async () => await handler.Handle(new GetGeoAreaQueryByIdQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*not enabled*");
    }

    [Fact]
    public async Task IngestLeadSources_ShouldFail_WhenManageFeatureDisabled()
    {
        var handler = new IngestLeadSourcesCommandHandler([], new ThrowingLeadSourceRecordRepository(), new ThrowingLeadSourceIngestionRunRepository(), new FakeTenantContext(), new DenyFeatureGate());

        var act = async () => await handler.Handle(new IngestLeadSourcesCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*not enabled*");
    }

    private sealed class DenyFeatureGate : IFeatureGate
    {
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken)
            => throw new DomainException($"Feature '{featureKey}' is not enabled for tenant '{tenantId}'.");

        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
    }

    private sealed class FakeTenantContext : GeoLeadsService.Api.ITenantContext
    {
        public Guid TenantId { get; } = Guid.NewGuid();
    }

    private sealed class ThrowingGeoLeadCatalog : IGeoLeadCatalog
    {
        public Task<IReadOnlyList<GeoLead>> SearchAsync(GeoPolygon geometry, IReadOnlyList<string> leadTypes, int limit, CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any catalog call is made.");
    }

    private sealed class ThrowingGeoAreaQueryRepository : IGeoAreaQueryRepository
    {
        public Task AddAsync(GeoAreaQuery query, CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any repository call is made.");

        public Task UpdateAsync(GeoAreaQuery query, CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any repository call is made.");

        public Task<GeoAreaQuery?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any repository call is made.");

        public Task<IReadOnlyList<GeoAreaQuery>> ListByTenantAsync(Guid tenantId, int limit, CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any repository call is made.");

        public Task<IReadOnlyList<GeoAreaQueryListItem>> ListSummariesByTenantAsync(Guid tenantId, int limit, CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any repository call is made.");
    }

    private sealed class ThrowingLeadSourceRecordRepository : ILeadSourceRecordRepository
    {
        public Task AddRangeAsync(IReadOnlyCollection<LeadSourceRecord> records, CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any repository call is made.");

        public Task UpsertRangeAsync(IReadOnlyCollection<LeadSourceRecord> records, CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any repository call is made.");

        public Task<IReadOnlyList<LeadSourceRecord>> ListAsync(CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any repository call is made.");

        public Task<IReadOnlyList<LeadSourceRecord>> ListRecentAsync(int limit, CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any repository call is made.");
    }

    private sealed class ThrowingLeadSourceIngestionRunRepository : ILeadSourceIngestionRunRepository
    {
        public Task AddAsync(LeadSourceIngestionRun run, CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any repository call is made.");

        public Task UpdateAsync(LeadSourceIngestionRun run, CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any repository call is made.");

        public Task<IReadOnlyList<LeadSourceIngestionRun>> ListRecentAsync(int limit, CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any repository call is made.");
    }
}
