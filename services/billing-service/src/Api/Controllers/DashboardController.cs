using BillingService.Application.Queries.GetBillingDashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("billing/dashboard")]
public sealed class DashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid tenantId, CancellationToken cancellationToken)
    {
        var data = await mediator.Send(new GetBillingDashboardQuery(tenantId), cancellationToken);
        return Ok(data);
    }
}
