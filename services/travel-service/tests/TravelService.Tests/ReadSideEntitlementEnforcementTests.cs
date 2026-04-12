using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Queries.ReportBookings;
using TravelService.Application.Queries.SearchTravel;

namespace TravelService.Tests;

public sealed class ReadSideEntitlementEnforcementTests
{
    [Fact]
    public async Task ReportBookings_ShouldFail_WhenFeatureDisabled()
    {
        var handler = new ReportBookingsQueryHandler(new ThrowingReadDbConnectionFactory(), new DenyFeatureGate());

        var act = async () => await handler.Handle(new ReportBookingsQuery(Guid.NewGuid(), "Pending", "Italy"), CancellationToken.None);

        await act.Should().ThrowAsync<TravelService.Domain.Exceptions.DomainException>().WithMessage("*not enabled*");
    }

    [Fact]
    public async Task SearchTravel_ShouldFail_WhenFeatureDisabled()
    {
        var handler = new SearchTravelQueryHandler(new ThrowingReadDbConnectionFactory(), new DenyFeatureGate());

        var act = async () => await handler.Handle(new SearchTravelQuery(Guid.NewGuid(), "Rome"), CancellationToken.None);

        await act.Should().ThrowAsync<TravelService.Domain.Exceptions.DomainException>().WithMessage("*not enabled*");
    }

    private sealed class DenyFeatureGate : IFeatureGate
    {
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken)
            => throw new TravelService.Domain.Exceptions.DomainException($"Feature '{featureKey}' is not enabled for tenant '{tenantId}'.");
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken)
            => throw new TravelService.Domain.Exceptions.DomainException($"Feature '{featureKey}' is not enabled for tenant '{tenantId}' and user '{userId}'.");
        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
    }

    private sealed class ThrowingReadDbConnectionFactory : IReadDbConnectionFactory
    {
        public Task<System.Data.IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken)
            => throw new InvalidOperationException("The feature gate should block before any DB call is made.");
    }
}
