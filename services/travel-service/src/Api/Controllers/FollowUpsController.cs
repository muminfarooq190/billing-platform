using TravelService.Api.Contracts;
using TravelService.Application.Commands.CreateFollowUp;
using TravelService.Application.Commands.UpdateFollowUp;
using TravelService.Application.Queries.GetFollowUpById;
using TravelService.Application.Queries.ListFollowUpsByTenant;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel/follow-ups")]
public sealed class FollowUpsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFollowUpRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateFollowUpCommand(
            request.TenantId, request.CustomerContactId, request.CustomerName,
            request.Subject, request.Notes, request.Priority, request.DueDate, request.AssignedToUserId), cancellationToken);
        return Created($"/travel/follow-ups/{id}", new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var model = await mediator.Send(new GetFollowUpByIdQuery(id), cancellationToken);
        return model is null ? NotFound() : Ok(model);
    }

    [HttpGet("tenant/{tenantId:guid}")]
    public async Task<IActionResult> ListByTenant(Guid tenantId, CancellationToken cancellationToken)
    {
        var models = await mediator.Send(new ListFollowUpsByTenantQuery(tenantId), cancellationToken);
        return Ok(models);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFollowUpRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateFollowUpCommand(id, request.Subject, request.Notes, request.Priority, request.DueDate, request.AssignedToUserId, request.Status), cancellationToken);
        return NoContent();
    }
}
