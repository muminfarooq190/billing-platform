using BillingService.Application.Abstractions;
using BillingService.Application.Commands.AssignUserFeatures;
using BillingService.Application.Queries.GetEffectiveEntitlements;
using BillingService.Application.ReadModels;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Enums;
using BillingService.Domain.Exceptions;
using BillingService.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Moq;

namespace BillingService.Tests.Application;

public sealed class AssignUserFeaturesCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateAssignment_WhenTenantOwnsFeatureAndFeatureIsAssignable()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var featureCatalogRepository = new Mock<IFeatureCatalogRepository>();
        var assignmentRepository = new Mock<ITenantUserFeatureAssignmentRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var mediator = new Mock<IMediator>();

        subscriptionRepository.Setup(x => x.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Subscription.Create(tenantId, PlanType.Pro, BillingCycle.Monthly));
        mediator.Setup(x => x.Send(It.IsAny<GetEffectiveEntitlementsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FeatureEntitlementReadModel> { new() { FeatureKey = "travel.booking.create", Granted = true } });
        featureCatalogRepository.Setup(x => x.GetByFeatureKeyAsync("travel.booking.create", It.IsAny<CancellationToken>()))
            .ReturnsAsync(FeatureCatalogEntry.Create("travel.booking.create", "travel-service", "travel", "Create booking", "desc", assignmentMode: FeatureAssignmentMode.ExplicitUserAssignment));
        assignmentRepository.Setup(x => x.GetActiveAssignmentAsync(tenantId, userId, "travel.booking.create", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantUserFeatureAssignment?)null);

        var handler = new AssignUserFeaturesCommandHandler(subscriptionRepository.Object, featureCatalogRepository.Object, assignmentRepository.Object, unitOfWork.Object, mediator.Object);
        var result = await handler.Handle(new AssignUserFeaturesCommand(tenantId, userId, new[] { "travel.booking.create" }, Guid.NewGuid(), null, null, null, null), CancellationToken.None);

        result.Should().ContainSingle().Which.Should().Be("travel.booking.create");
        assignmentRepository.Verify(x => x.AddAsync(It.IsAny<TenantUserFeatureAssignment>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRejectTenantWideFeature()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var featureCatalogRepository = new Mock<IFeatureCatalogRepository>();
        var assignmentRepository = new Mock<ITenantUserFeatureAssignmentRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var mediator = new Mock<IMediator>();

        subscriptionRepository.Setup(x => x.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Subscription.Create(tenantId, PlanType.Pro, BillingCycle.Monthly));
        mediator.Setup(x => x.Send(It.IsAny<GetEffectiveEntitlementsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FeatureEntitlementReadModel> { new() { FeatureKey = "travel.timeline.read", Granted = true } });
        featureCatalogRepository.Setup(x => x.GetByFeatureKeyAsync("travel.timeline.read", It.IsAny<CancellationToken>()))
            .ReturnsAsync(FeatureCatalogEntry.Create("travel.timeline.read", "travel-service", "travel", "Read timeline", "desc", assignmentMode: FeatureAssignmentMode.TenantWide));

        var handler = new AssignUserFeaturesCommandHandler(subscriptionRepository.Object, featureCatalogRepository.Object, assignmentRepository.Object, unitOfWork.Object, mediator.Object);

        var act = () => handler.Handle(new AssignUserFeaturesCommand(tenantId, userId, new[] { "travel.timeline.read" }, null, null, null, null, null), CancellationToken.None);
        await act.Should().ThrowAsync<DomainException>();
    }
}
