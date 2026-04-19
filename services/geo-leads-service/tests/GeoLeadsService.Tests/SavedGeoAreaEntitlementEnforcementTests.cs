using FluentAssertions;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Application.Commands.CreateSavedGeoArea;
using GeoLeadsService.Application.Commands.DeleteSavedGeoArea;
using GeoLeadsService.Application.Commands.RunSavedGeoAreaQuery;
using GeoLeadsService.Application.Commands.UpdateSavedGeoArea;
using GeoLeadsService.Application.Queries.GetSavedGeoAreaById;
using GeoLeadsService.Application.Queries.ListSavedGeoAreas;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Exceptions;
using GeoLeadsService.Domain.Repositories;
using MediatR;
using Xunit;

namespace GeoLeadsService.Tests;

public sealed class SavedGeoAreaEntitlementEnforcementTests
{
    [Fact]
    public async Task CreateSavedGeoArea_ShouldFail_WhenManageFeatureDisabled()
    {
        var handler = new CreateSavedGeoAreaCommandHandler(new ThrowingSavedGeoAreaRepository(), new DenyFeatureGate());
        var polygon = BuildPolygon();

        var act = async () => await handler.Handle(new CreateSavedGeoAreaCommand(Guid.NewGuid(), "Area", polygon), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*not enabled*");
    }

    [Fact]
    public async Task ListSavedGeoAreas_ShouldFail_WhenReadFeatureDisabled()
    {
        var handler = new ListSavedGeoAreasQueryHandler(new ThrowingSavedGeoAreaRepository(), new DenyFeatureGate());

        var act = async () => await handler.Handle(new ListSavedGeoAreasQuery(Guid.NewGuid(), 10), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*not enabled*");
    }

    [Fact]
    public async Task GetSavedGeoAreaById_ShouldFail_WhenReadFeatureDisabled()
    {
        var handler = new GetSavedGeoAreaByIdQueryHandler(new ThrowingSavedGeoAreaRepository(), new DenyFeatureGate());

        var act = async () => await handler.Handle(new GetSavedGeoAreaByIdQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*not enabled*");
    }

    [Fact]
    public async Task UpdateSavedGeoArea_ShouldFail_WhenManageFeatureDisabled()
    {
        var handler = new UpdateSavedGeoAreaCommandHandler(new ThrowingSavedGeoAreaRepository(), new DenyFeatureGate());

        var act = async () => await handler.Handle(new UpdateSavedGeoAreaCommand(Guid.NewGuid(), Guid.NewGuid(), "Area", BuildPolygon()), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*not enabled*");
    }

    [Fact]
    public async Task DeleteSavedGeoArea_ShouldFail_WhenManageFeatureDisabled()
    {
        var handler = new DeleteSavedGeoAreaCommandHandler(new ThrowingSavedGeoAreaRepository(), new DenyFeatureGate());

        var act = async () => await handler.Handle(new DeleteSavedGeoAreaCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*not enabled*");
    }

    private static GeoPolygon BuildPolygon()
        => new([
            new GeoCoordinate(72.82m, 18.92m),
            new GeoCoordinate(72.84m, 18.92m),
            new GeoCoordinate(72.84m, 18.94m),
            new GeoCoordinate(72.82m, 18.94m),
            new GeoCoordinate(72.82m, 18.92m)
        ]);

    private sealed class DenyFeatureGate : IFeatureGate
    {
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken)
            => throw new DomainException($"Feature '{featureKey}' is not enabled for tenant '{tenantId}'.");

        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
    }

    private sealed class ThrowingSavedGeoAreaRepository : ISavedGeoAreaRepository
    {
        public Task AddAsync(SavedGeoArea area, CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any repository call is made.");
        public Task UpdateAsync(SavedGeoArea area, CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any repository call is made.");
        public Task DeleteAsync(SavedGeoArea area, CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any repository call is made.");
        public Task<SavedGeoArea?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any repository call is made.");
        public Task<IReadOnlyList<SavedGeoArea>> ListByTenantAsync(Guid tenantId, int limit, CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any repository call is made.");
    }

}
