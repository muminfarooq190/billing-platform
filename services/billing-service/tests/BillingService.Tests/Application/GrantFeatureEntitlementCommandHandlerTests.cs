using BillingService.Application.Abstractions;
using BillingService.Application.Commands.GrantFeatureEntitlement;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using FluentAssertions;

namespace BillingService.Tests.Application;

public sealed class GrantFeatureEntitlementCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateAdminGrant_WithLimitAndReason()
    {
        var repository = new RecordingFeatureEntitlementRepository();
        var handler = new GrantFeatureEntitlementCommandHandler(repository, new NoOpUnitOfWork());
        var tenantId = Guid.NewGuid();

        var result = await handler.Handle(new GrantFeatureEntitlementCommand(
            tenantId,
            "communication.notification.send",
            true,
            10000,
            DateTimeOffset.UtcNow,
            null,
            "sales-approved uplift"), CancellationToken.None);

        repository.Items.Should().ContainSingle();
        repository.Items[0].TenantId.Should().Be(tenantId);
        repository.Items[0].FeatureKey.Should().Be("communication.notification.send");
        repository.Items[0].LimitValue.Should().Be(10000);
        result.Source.Should().Be("AdminGrant");
    }

    private sealed class RecordingFeatureEntitlementRepository : IFeatureEntitlementRepository
    {
        public List<FeatureEntitlement> Items { get; } = [];
        public Task AddRangeAsync(IReadOnlyCollection<FeatureEntitlement> entitlements, CancellationToken cancellationToken)
        {
            Items.AddRange(entitlements);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<FeatureEntitlement>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<FeatureEntitlement>>(Items.Where(x => x.TenantId == tenantId).ToList());
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
    }
}
