using MediatR;
using Microsoft.AspNetCore.Mvc;
using TravelService.Api.Contracts;
using TravelService.Application.Commands.TravelInquiries;
using TravelService.Application.Queries.TravelInquiries;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel/inquiries")]
public sealed class InquiriesController(IMediator mediator, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? source = null,
        [FromQuery] Guid? assignedToUserId = null,
        [FromQuery] string? destination = null,
        [FromQuery(Name = "q")] string? query = null,
        CancellationToken cancellationToken = default)
    {
        var model = await mediator.Send(new ListTravelInquiriesQuery(tenantContext.TenantId, page, pageSize, status, source, assignedToUserId, destination, query), cancellationToken);
        return Ok(model);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var model = await mediator.Send(new GetTravelInquiryByIdQuery(tenantContext.TenantId, id), cancellationToken);
        return model is null ? NotFound() : Ok(model);
    }

    [HttpGet("{id:guid}/history")]
    public async Task<IActionResult> GetHistory(Guid id, CancellationToken cancellationToken)
    {
        var model = await mediator.Send(new GetTravelInquiryHistoryQuery(tenantContext.TenantId, id), cancellationToken);
        return Ok(model);
    }

    [HttpPost("{id:guid}/assign")]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignInquiryRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new AssignInquiryCommand(tenantContext.TenantId, id, request.AssignedToUserId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/qualify")]
    public async Task<IActionResult> Qualify(Guid id, [FromBody] InquiryStatusReasonRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new QualifyInquiryCommand(tenantContext.TenantId, id, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/disqualify")]
    public async Task<IActionResult> Disqualify(Guid id, [FromQuery] string status, [FromBody] InquiryStatusReasonRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new DisqualifyInquiryCommand(tenantContext.TenantId, id, status, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/mark-contacted")]
    public async Task<IActionResult> MarkContacted(Guid id, [FromBody] InquiryStatusReasonRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new MarkInquiryContactedCommand(tenantContext.TenantId, id, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> Archive(Guid id, [FromBody] InquiryStatusReasonRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new ArchiveInquiryCommand(tenantContext.TenantId, id, request.Reason), cancellationToken);
        return NoContent();
    }
}
