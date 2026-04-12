using BillingService.Api.Contracts;
using BillingService.Api.Controllers;
using BillingService.Application.Abstractions;
using BillingService.Application.Queries.GetEffectiveEntitlements;
using BillingService.Application.ReadModels;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Enums;
using BillingService.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BillingService.Tests.Api;

public sealed class FlexibleEntitlementsAdminControllerTests
{
    [Fact]
    public async Task FeaturesController_CreateAndUpdate_ShouldPersistEntries()
    {
        var repo = new InMemoryFeatureCatalogRepository();
        var controller = new FeaturesController(repo, new FakeUnitOfWork());

        var createResult = await controller.Create(new CreateFeatureCatalogEntryRequest(
            "travel.audit.read", "travel-service", "travel", "Audit Read", "Allows audit access", false, null, "{}", "ExplicitUserAssignment", null), CancellationToken.None);

        createResult.Should().BeOfType<CreatedAtActionResult>();
        repo.Items.Should().ContainSingle(x => x.FeatureKey == "travel.audit.read");

        var updateResult = await controller.Update("travel.audit.read", new UpdateFeatureCatalogEntryRequest(
            "travel-service", "reporting", "Audit & Reporting", "Updated", false, null, "{\"v\":2}", "SeatLimitedAssignment", 5), CancellationToken.None);

        updateResult.Should().BeOfType<OkObjectResult>();
        repo.Items.Single().Category.Should().Be("reporting");
        repo.Items.Single().DisplayName.Should().Be("Audit & Reporting");
    }

    [Fact]
    public async Task PackagesController_ReplaceFeatures_ShouldSwapPackageFeatures()
    {
        var repo = new InMemoryCommercialPackageRepository();
        var package = CommercialPackage.Create("addon.audit", "Audit", "Addon", "Flat", "Audit add-on");
        repo.Packages.Add(package);
        repo.Features.Add(CommercialPackageFeature.Create(package.Id, "travel.audit.read", true));

        var controller = new PackagesController(repo, new FakeUnitOfWork());
        var result = await controller.ReplaceFeatures(package.Id, new ReplaceCommercialPackageFeaturesRequest([
            new CommercialPackageFeatureRequest("travel.audit.read", true, null, LimitMergePolicy.Max, null),
            new CommercialPackageFeatureRequest("communication.notification.send", true, 1000, LimitMergePolicy.Sum, null)
        ]), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        repo.Features.Should().HaveCount(2);
        repo.Features.Should().Contain(x => x.FeatureKey == "communication.notification.send" && x.LimitValue == 1000 && x.LimitMergePolicy == LimitMergePolicy.Sum);
    }

    [Fact]
    public async Task TenantBillingController_ShouldManageAssignmentsOverrides_AndFeatureLookup()
    {
        var tenantId = Guid.NewGuid();
        var packageRepo = new InMemoryCommercialPackageRepository();
        var assignmentRepo = new InMemoryTenantSubscriptionPackageRepository();
        var overrideRepo = new InMemoryTenantFeatureOverrideRepository();
        var package = CommercialPackage.Create("addon.audit", "Audit", "Addon", "Flat", "Audit add-on");
        packageRepo.Packages.Add(package);

        var mediator = new Mock<IMediator>();
        mediator
            .Setup(x => x.Send(It.IsAny<GetEffectiveEntitlementsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FeatureEntitlementReadModel>
            {
                new() { FeatureKey = "travel.audit.read", Granted = true, Source = "Package" }
            });

        var controller = new TenantBillingController(assignmentRepo, overrideRepo, packageRepo, new FakeUnitOfWork(), mediator.Object);

        var createAssignment = await controller.CreatePackage(tenantId, new AssignTenantPackageRequest(package.Id, "Sales", "Active", DateTimeOffset.UtcNow.AddDays(-1), null, null), CancellationToken.None);
        createAssignment.Should().BeOfType<CreatedAtActionResult>();
        var assignment = assignmentRepo.Items.Single();

        var updateAssignment = await controller.UpdatePackage(tenantId, assignment.Id, new UpdateTenantPackageRequest(package.Id, "Support", "Active", assignment.EffectiveFrom, assignment.EffectiveFrom.AddDays(30), "{}"), CancellationToken.None);
        updateAssignment.Should().BeOfType<OkObjectResult>();
        assignment.Source.Should().Be("Support");

        var createOverride = await controller.CreateFeatureOverride(tenantId, new CreateTenantFeatureOverrideRequest("travel.audit.read", false, null, "Contract restriction", "Sales", "alice", DateTimeOffset.UtcNow.AddMinutes(-1), null, null), CancellationToken.None);
        createOverride.Should().BeOfType<CreatedAtActionResult>();
        var overrideEntry = overrideRepo.Items.Single();

        var updateOverride = await controller.UpdateFeatureOverride(tenantId, overrideEntry.Id, new UpdateTenantFeatureOverrideRequest("travel.audit.read", true, 5, "Manual fix", "Support", "bob", DateTimeOffset.UtcNow.AddMinutes(-1), null, "{}"), CancellationToken.None);
        updateOverride.Should().BeOfType<OkObjectResult>();
        overrideEntry.Granted.Should().BeTrue();
        overrideEntry.LimitValue.Should().Be(5);

        var entitlementResult = await controller.GetEntitlement(tenantId, "travel.audit.read", CancellationToken.None);
        entitlementResult.Should().BeOfType<OkObjectResult>();

        (await controller.DeleteFeatureOverride(tenantId, overrideEntry.Id, CancellationToken.None)).Should().BeOfType<NoContentResult>();
        overrideEntry.DeletedAt.Should().NotBeNull();

        (await controller.DeletePackage(tenantId, assignment.Id, CancellationToken.None)).Should().BeOfType<NoContentResult>();
        assignment.DeletedAt.Should().NotBeNull();
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
    }

    private sealed class InMemoryFeatureCatalogRepository : IFeatureCatalogRepository
    {
        public List<FeatureCatalogEntry> Items { get; } = [];
        public Task<IReadOnlyList<FeatureCatalogEntry>> ListAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<FeatureCatalogEntry>>(Items.ToList());
        public Task<FeatureCatalogEntry?> GetByFeatureKeyAsync(string featureKey, CancellationToken cancellationToken) => Task.FromResult(Items.FirstOrDefault(x => x.FeatureKey == featureKey));
        public Task AddAsync(FeatureCatalogEntry entry, CancellationToken cancellationToken) { Items.Add(entry); return Task.CompletedTask; }
    }

    private sealed class InMemoryCommercialPackageRepository : ICommercialPackageRepository
    {
        public List<CommercialPackage> Packages { get; } = [];
        public List<CommercialPackageFeature> Features { get; } = [];
        public Task<IReadOnlyList<CommercialPackage>> ListAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<CommercialPackage>>(Packages.ToList());
        public Task<IReadOnlyList<CommercialPackage>> ListActiveAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<CommercialPackage>>(Packages.Where(x => x.IsActive).ToList());
        public Task<CommercialPackage?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Packages.FirstOrDefault(x => x.Id == id));
        public Task<IReadOnlyList<CommercialPackageFeature>> ListFeaturesByPackageIdsAsync(IReadOnlyCollection<Guid> packageIds, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<CommercialPackageFeature>>(Features.Where(x => packageIds.Contains(x.CommercialPackageId)).ToList());
        public Task<IReadOnlyList<CommercialPackageFeature>> ListFeaturesByPackageIdAsync(Guid packageId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<CommercialPackageFeature>>(Features.Where(x => x.CommercialPackageId == packageId).ToList());
        public Task AddAsync(CommercialPackage package, CancellationToken cancellationToken) { Packages.Add(package); return Task.CompletedTask; }
        public Task AddRangeAsync(IReadOnlyCollection<CommercialPackage> packages, CancellationToken cancellationToken) { Packages.AddRange(packages); return Task.CompletedTask; }
        public Task AddFeaturesAsync(IReadOnlyCollection<CommercialPackageFeature> features, CancellationToken cancellationToken) { Features.AddRange(features); return Task.CompletedTask; }
        public Task RemoveFeaturesAsync(IReadOnlyCollection<CommercialPackageFeature> features, CancellationToken cancellationToken) { foreach (var item in features) Features.Remove(item); return Task.CompletedTask; }
    }

    private sealed class InMemoryTenantSubscriptionPackageRepository : ITenantSubscriptionPackageRepository
    {
        public List<TenantSubscriptionPackage> Items { get; } = [];
        public Task<IReadOnlyList<TenantSubscriptionPackage>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<TenantSubscriptionPackage>>(Items.Where(x => x.TenantId == tenantId && x.DeletedAt == null).ToList());
        public Task<TenantSubscriptionPackage?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Items.FirstOrDefault(x => x.Id == id && x.DeletedAt == null));
        public Task AddAsync(TenantSubscriptionPackage assignment, CancellationToken cancellationToken) { Items.Add(assignment); return Task.CompletedTask; }
        public Task AddRangeAsync(IReadOnlyCollection<TenantSubscriptionPackage> assignments, CancellationToken cancellationToken) { Items.AddRange(assignments); return Task.CompletedTask; }
    }

    private sealed class InMemoryTenantFeatureOverrideRepository : ITenantFeatureOverrideRepository
    {
        public List<TenantFeatureOverride> Items { get; } = [];
        public Task AddAsync(TenantFeatureOverride entry, CancellationToken cancellationToken) { Items.Add(entry); return Task.CompletedTask; }
        public Task AddRangeAsync(IReadOnlyCollection<TenantFeatureOverride> overrides, CancellationToken cancellationToken) { Items.AddRange(overrides); return Task.CompletedTask; }
        public Task<TenantFeatureOverride?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Items.FirstOrDefault(x => x.Id == id && x.DeletedAt == null));
        public Task<IReadOnlyList<TenantFeatureOverride>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<TenantFeatureOverride>>(Items.Where(x => x.TenantId == tenantId && x.DeletedAt == null).ToList());
    }

}
