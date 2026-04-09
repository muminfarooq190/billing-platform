using BillingService.Application.Queries.GetEffectiveEntitlements;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("billing/entitlements")]
public sealed class EntitlementsController(IMediator mediator, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var model = await mediator.Send(new GetEffectiveEntitlementsQuery(tenantContext.TenantId), cancellationToken);
        return Ok(model);
    }

    [HttpGet("{tenantId:guid}")]
    public async Task<IActionResult> GetByTenant(Guid tenantId, CancellationToken cancellationToken)
    {
        var model = await mediator.Send(new GetEffectiveEntitlementsQuery(tenantId), cancellationToken);
        return Ok(model);
    }
}
