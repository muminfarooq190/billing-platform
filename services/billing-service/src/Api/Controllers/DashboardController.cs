using BillingService.Application.Queries.GetBillingDashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("billing/dashboard")]
public sealed class DashboardController(IMediator mediator, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var data = await mediator.Send(new GetBillingDashboardQuery(tenantContext.TenantId), cancellationToken);
        return Ok(data);
    }
}
