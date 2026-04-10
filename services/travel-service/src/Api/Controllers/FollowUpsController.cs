using TravelService.Api.Contracts;
using TravelService.Application.Commands.CreateFollowUp;
using TravelService.Application.Commands.UpdateFollowUp;
using TravelService.Application.Queries.GetFollowUpById;
using TravelService.Application.Queries.ListFollowUpsByTenant;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TravelService.Application.Commands.FollowUps;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel/follow-ups")]
public sealed class FollowUpsController(IMediator mediator, ITenantContext tenantContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFollowUpRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateFollowUpCommand(
            tenantContext.TenantId, request.CustomerContactId, request.CustomerName,
            request.Subject, request.Notes, request.Priority, request.DueDate, request.AssignedToUserId), cancellationToken);
        return Created($"/travel/follow-ups/{id}", new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var model = await mediator.Send(new GetFollowUpByIdQuery(id), cancellationToken);
        return model is null ? NotFound() : Ok(model);
    }

    [HttpGet]
    public async Task<IActionResult> ListByTenant(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? customerName = null,
        [FromQuery] DateTimeOffset? dueDateFrom = null,
        [FromQuery] DateTimeOffset? dueDateTo = null,
        CancellationToken cancellationToken = default)
    {
        var models = await mediator.Send(new ListFollowUpsByTenantQuery(tenantContext.TenantId, page, pageSize, status, customerName, dueDateFrom, dueDateTo), cancellationToken);
        return Ok(models);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFollowUpRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateFollowUpCommand(id, request.Subject, request.Notes, request.Priority, request.DueDate, request.AssignedToUserId, request.Status), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new CompleteFollowUpCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reassign")]
    public async Task<IActionResult> Reassign(Guid id, [FromBody] ReassignFollowUpRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new ReassignFollowUpCommand(id, request.AssignedToUserId), cancellationToken);
        return NoContent();
    }
}
