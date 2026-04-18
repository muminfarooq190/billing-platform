using BillingService.Application.Commands.GenerateInvoice;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Enums;
using BillingService.Domain.Repositories;
using FluentAssertions;

namespace BillingService.Tests.Application;

public sealed class BillingPricingMetadataTests
{
    [Fact]
    public async Task BillingPricingResolver_ShouldUsePackageMetadataPricing_WhenAssignmentExists()
    {
        var subscription = Subscription.Create(Guid.NewGuid(), PlanType.Pro, BillingCycle.Monthly);
        var package = CommercialPackage.Create(
            "growth.v1",
            "Growth",
            "BasePlan",
            "Flat",
            "Growth package",
            true,
            "{\"pricing\":{\"monthly\":{\"amount\":129,\"currency\":\"USD\"},\"annual\":{\"amount\":1290,\"currency\":\"USD\"}},\"taxRate\":0.15}");
        var assignment = TenantSubscriptionPackage.Create(subscription.TenantId, package.Id, "Test", "Active", subscription.StartDate);
        var resolver = new BillingPricingResolver(new AssignedTenantPackageRepository(assignment), new SinglePackageRepository(package));

        var pricing = await resolver.ResolveAsync(subscription, CancellationToken.None);

        pricing.LineItems.Should().ContainSingle();
        pricing.LineItems[0].UnitPrice.Amount.Should().Be(129m);
        pricing.TaxAmount.Amount.Should().Be(19.35m);
        pricing.PricingReference.Should().Be("package:growth.v1");
    }

    private sealed class AssignedTenantPackageRepository(TenantSubscriptionPackage assignment) : ITenantSubscriptionPackageRepository
    {
        public Task<IReadOnlyList<TenantSubscriptionPackage>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken)
            => Task.FromResult((IReadOnlyList<TenantSubscriptionPackage>)(tenantId == assignment.TenantId ? new[] { assignment } : []));
        public Task<TenantSubscriptionPackage?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<TenantSubscriptionPackage?>(assignment.Id == id ? assignment : null);
        public Task AddAsync(TenantSubscriptionPackage assignment, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task AddRangeAsync(IReadOnlyCollection<TenantSubscriptionPackage> assignments, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class SinglePackageRepository(CommercialPackage package) : ICommercialPackageRepository
    {
        public Task<IReadOnlyList<CommercialPackage>> ListAsync(CancellationToken cancellationToken) => Task.FromResult((IReadOnlyList<CommercialPackage>)[package]);
        public Task<IReadOnlyList<CommercialPackage>> ListActiveAsync(CancellationToken cancellationToken) => Task.FromResult((IReadOnlyList<CommercialPackage>)[package]);
        public Task<CommercialPackage?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<CommercialPackage?>(package.Id == id ? package : null);
        public Task<IReadOnlyList<CommercialPackageFeature>> ListFeaturesByPackageIdsAsync(IReadOnlyCollection<Guid> packageIds, CancellationToken cancellationToken) => Task.FromResult((IReadOnlyList<CommercialPackageFeature>)[]);
        public Task<IReadOnlyList<CommercialPackageFeature>> ListFeaturesByPackageIdAsync(Guid packageId, CancellationToken cancellationToken) => Task.FromResult((IReadOnlyList<CommercialPackageFeature>)[]);
        public Task AddAsync(CommercialPackage package, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task AddRangeAsync(IReadOnlyCollection<CommercialPackage> packages, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task AddFeaturesAsync(IReadOnlyCollection<CommercialPackageFeature> features, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task RemoveFeaturesAsync(IReadOnlyCollection<CommercialPackageFeature> features, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
