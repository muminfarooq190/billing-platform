using BillingService.Api.Contracts;
using BillingService.Application.Commands.CancelSubscription;
using BillingService.Application.Commands.CreateSubscription;
using BillingService.Application.Commands.ReactivateSubscription;
using BillingService.Application.Queries.GetSubscriptionByTenant;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("billing/subscriptions")]
public sealed class SubscriptionsController(IMediator mediator, ITenantContext tenantContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateSubscriptionCommand(tenantContext.TenantId, request.PlanType, request.BillingCycle), cancellationToken);
        return Created($"/billing/subscriptions/{id}", new { id });
    }

    [HttpGet]
    public async Task<IActionResult> GetByTenant(CancellationToken cancellationToken)
    {
        var model = await mediator.Send(new GetSubscriptionByTenantQuery(tenantContext.TenantId), cancellationToken);
        return model is null ? NotFound() : Ok(model);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new CancelSubscriptionCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Reactivate a Cancelled or PastDue subscription before its period
    /// elapses. Backs the portal "Reactivate" CTA on the
    /// cancelled-ending / past-due banner.
    /// </summary>
    [HttpPost("{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new ReactivateSubscriptionCommand(id), cancellationToken);
        return NoContent();
    }
}
