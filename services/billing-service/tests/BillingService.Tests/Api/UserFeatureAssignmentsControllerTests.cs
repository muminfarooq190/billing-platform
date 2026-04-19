using BillingService.Api;
using BillingService.Api.Controllers;
using BillingService.Api.Contracts;
using BillingService.Application.Commands.AssignUserFeatures;
using BillingService.Application.Commands.RevokeUserFeatureAssignment;
using BillingService.Application.Queries.GetMyFeatureAccess;
using BillingService.Application.Queries.GetTenantFeatureAllocations;
using BillingService.Application.Queries.GetUserFeatureAccess;
using BillingService.Application.ReadModels;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BillingService.Tests.Api;

public sealed class UserFeatureAssignmentsControllerTests
{
    [Fact]
    public async Task GetTenantFeatureAllocations_ShouldForbid_CrossTenantAccess()
    {
        var routeTenantId = Guid.NewGuid();
        var contextTenantId = Guid.NewGuid();
        var mediator = new Mock<IMediator>();
        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.TenantId).Returns(contextTenantId);

        var controller = new UserFeatureAssignmentsController(mediator.Object, tenantContext.Object);
        var result = await controller.GetTenantFeatureAllocations(routeTenantId, CancellationToken.None);

        result.Should().BeOfType<ForbidResult>();
        mediator.Verify(x => x.Send(It.IsAny<GetTenantFeatureAllocationsQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AssignFeatures_ShouldForwardTenantContextUserIdWhenRequestDoesNotProvideOne()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var actingUserId = Guid.NewGuid();
        var mediator = new Mock<IMediator>();
        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.TenantId).Returns(tenantId);
        tenantContext.SetupGet(x => x.UserId).Returns(actingUserId);

        mediator.Setup(x => x.Send(It.IsAny<AssignUserFeaturesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "travel.booking.create" });

        var controller = new UserFeatureAssignmentsController(mediator.Object, tenantContext.Object);
        var request = new AssignUserFeaturesRequest { FeatureKeys = new[] { "travel.booking.create" } };

        var result = await controller.AssignFeatures(tenantId, userId, request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        mediator.Verify(x => x.Send(It.Is<AssignUserFeaturesCommand>(c => c.TenantId == tenantId && c.UserId == userId && c.AssignedByUserId == actingUserId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMyFeatureAccess_ShouldUseTenantAndUserFromContext()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var mediator = new Mock<IMediator>();
        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.TenantId).Returns(tenantId);
        tenantContext.SetupGet(x => x.UserId).Returns(userId);
        mediator.Setup(x => x.Send(It.Is<GetMyFeatureAccessQuery>(q => q.TenantId == tenantId && q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserFeatureAccessReadModel>());

        var controller = new UserFeatureAssignmentsController(mediator.Object, tenantContext.Object);
        var result = await controller.GetMyFeatureAccess(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AssignFeatures_ShouldForbid_CrossTenantAccess()
    {
        var routeTenantId = Guid.NewGuid();
        var contextTenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var mediator = new Mock<IMediator>();
        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.TenantId).Returns(contextTenantId);
        tenantContext.SetupGet(x => x.UserId).Returns(Guid.NewGuid());

        var controller = new UserFeatureAssignmentsController(mediator.Object, tenantContext.Object);
        var result = await controller.AssignFeatures(routeTenantId, userId, new AssignUserFeaturesRequest { FeatureKeys = new[] { "travel.booking.create" } }, CancellationToken.None);

        result.Should().BeOfType<ForbidResult>();
        mediator.Verify(x => x.Send(It.IsAny<AssignUserFeaturesCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeFeature_ShouldForwardTenantContextUserId()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var actingUserId = Guid.NewGuid();
        var mediator = new Mock<IMediator>();
        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.TenantId).Returns(tenantId);
        tenantContext.SetupGet(x => x.UserId).Returns(actingUserId);

        var controller = new UserFeatureAssignmentsController(mediator.Object, tenantContext.Object);
        var result = await controller.RevokeFeature(tenantId, userId, "travel.booking.create", CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        mediator.Verify(x => x.Send(It.Is<RevokeUserFeatureAssignmentCommand>(c => c.TenantId == tenantId && c.UserId == userId && c.RevokedByUserId == actingUserId), It.IsAny<CancellationToken>()), Times.Once);
    }
}
