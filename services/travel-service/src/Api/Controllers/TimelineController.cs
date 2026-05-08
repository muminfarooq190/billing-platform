using MediatR;
using Microsoft.AspNetCore.Mvc;
using TravelService.Api.Auth;
using TravelService.Application.Abstractions;
using TravelService.Application.Queries.GetTimeline;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel")]
[RequirePermission(Permissions.Travel.TimelineRead)]
public sealed class TimelineController(IMediator mediator, ITenantContext tenantContext, IFeatureGate featureGate) : ControllerBase
{
    [HttpGet("activity")]
    public async Task<IActionResult> ListTenantActivity(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? entityType = null,
        [FromQuery] string? activityType = null,
        [FromQuery] Guid? actorUserId = null,
        [FromQuery] DateTimeOffset? fromDate = null,
        [FromQuery] DateTimeOffset? toDate = null,
        CancellationToken cancellationToken = default)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.TravelTimelineRead, tenantContext.TenantId, tenantContext.UserId, cancellationToken);
        var result = await mediator.Send(
            new ListTenantActivityQuery(tenantContext.TenantId, page, pageSize, entityType, activityType, actorUserId, fromDate, toDate),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("timeline/{entityType}/{entityId:guid}")]
    public async Task<IActionResult> GetTimeline(string entityType, Guid entityId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.TravelTimelineRead, tenantContext.TenantId, tenantContext.UserId, cancellationToken);
        var result = await mediator.Send(new GetTimelineQuery(tenantContext.TenantId, entityType, entityId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("quotations/{id:guid}/timeline")]
    public Task<IActionResult> GetQuotationTimeline(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => GetTimeline("Quotation", id, page, pageSize, cancellationToken);

    [HttpGet("bookings/{id:guid}/timeline")]
    public Task<IActionResult> GetBookingTimeline(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => GetTimeline("Booking", id, page, pageSize, cancellationToken);
}
