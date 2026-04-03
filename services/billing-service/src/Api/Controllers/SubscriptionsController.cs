using BillingService.Api.Contracts;
using BillingService.Application.Commands.CancelSubscription;
using BillingService.Application.Commands.CreateSubscription;
using BillingService.Application.Queries.GetSubscriptionByTenant;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("billing/subscriptions")]
public sealed class SubscriptionsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateSubscriptionCommand(request.TenantId, request.PlanType, request.BillingCycle), cancellationToken);
        return Created($"/billing/subscriptions/{id}", new { id });
    }

    [HttpGet("{tenantId:guid}")]
    public async Task<IActionResult> GetByTenant(Guid tenantId, CancellationToken cancellationToken)
    {
        var model = await mediator.Send(new GetSubscriptionByTenantQuery(tenantId), cancellationToken);
        return model is null ? NotFound() : Ok(model);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new CancelSubscriptionCommand(id), cancellationToken);
        return NoContent();
    }
}
