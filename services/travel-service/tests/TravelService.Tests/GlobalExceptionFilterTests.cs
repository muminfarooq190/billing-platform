using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using TravelService.Api.Filters;
using TravelService.Domain.Exceptions;

namespace TravelService.Tests;

public sealed class GlobalExceptionFilterTests
{
    [Fact]
    public void OnException_ShouldMapEntitlementDomainException_To403()
    {
        var filter = new GlobalExceptionFilter();
        var context = new ExceptionContext(CreateActionContext(), []);
        context.Exception = new DomainException("Feature 'travel.audit.read' is not enabled for tenant 'tenant-1'.");

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        var result = context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be(403);
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(new { code = "entitlement_denied", message = "Feature 'travel.audit.read' is not enabled for tenant 'tenant-1'." });
    }

    [Fact]
    public void OnException_ShouldMapRegularDomainException_To400ProblemDetails()
    {
        var filter = new GlobalExceptionFilter();
        var context = new ExceptionContext(CreateActionContext(), []);
        context.Exception = new DomainException("Validation exploded.");

        filter.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        var result = context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be(400);
        var details = result.Value.Should().BeOfType<ProblemDetails>().Subject;
        details.Status.Should().Be(400);
        details.Detail.Should().Be("Validation exploded.");
    }

    private static ActionContext CreateActionContext()
        => new(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
}
