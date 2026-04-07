using MediatR;
using Microsoft.AspNetCore.Mvc;
using TravelService.Application.Queries.GetTimeline;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel")]
public sealed class TimelineController(IMediator mediator, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet("timeline/{entityType}/{entityId:guid}")]
    public async Task<IActionResult> GetTimeline(string entityType, Guid entityId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
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
