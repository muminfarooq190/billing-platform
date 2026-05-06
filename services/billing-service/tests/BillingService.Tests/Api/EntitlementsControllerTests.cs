using BillingService.Api;
using BillingService.Api.Controllers;
using BillingService.Application.Abstractions;
using BillingService.Application.Queries.GetEffectiveEntitlements;
using BillingService.Application.ReadModels;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BillingService.Tests.Api;

public sealed class EntitlementsControllerTests
{
    [Fact]
    public async Task GetMine_ShouldResolveUsingTenantContextTenantId()
    {
        var tenantId = Guid.NewGuid();
        var mediator = new Mock<IMediator>();
        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.TenantId).Returns(tenantId);

        mediator
            .Setup(x => x.Send(It.Is<GetEffectiveEntitlementsQuery>(q => q.TenantId == tenantId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FeatureEntitlementReadModel>
            {
                new() { FeatureKey = "travel.audit.read", Granted = true, Source = "Package" }
            });

        var readDb = new Mock<IReadDbConnectionFactory>();
        var controller = new EntitlementsController(mediator.Object, tenantContext.Object, readDb.Object);

        var result = await controller.GetMine(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var model = ok.Value.Should().BeAssignableTo<IReadOnlyList<FeatureEntitlementReadModel>>().Subject;
        model.Should().ContainSingle(x => x.FeatureKey == "travel.audit.read" && x.Granted);
        mediator.Verify(x => x.Send(It.Is<GetEffectiveEntitlementsQuery>(q => q.TenantId == tenantId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByTenant_ShouldForbid_WhenRouteTenantDoesNotMatchContextTenant()
    {
        var routeTenantId = Guid.NewGuid();
        var contextTenantId = Guid.NewGuid();
        var mediator = new Mock<IMediator>();
        var tenantContext = new Mock<ITenantContext>();
        var readDb = new Mock<IReadDbConnectionFactory>();
        tenantContext.SetupGet(x => x.TenantId).Returns(contextTenantId);

        var controller = new EntitlementsController(mediator.Object, tenantContext.Object, readDb.Object);

        var result = await controller.GetByTenant(routeTenantId, CancellationToken.None);

        result.Should().BeOfType<ForbidResult>();
        mediator.Verify(x => x.Send(It.IsAny<GetEffectiveEntitlementsQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
