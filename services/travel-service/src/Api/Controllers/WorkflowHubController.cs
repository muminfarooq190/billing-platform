using MediatR;
using Microsoft.AspNetCore.Mvc;
using TravelService.Api.Auth;
using TravelService.Application.Queries.GetWorkQueue;
using TravelService.Application.Abstractions;

namespace TravelService.Api.Controllers;

[ApiController]
[Route("travel/workflow-hub")]
[RequirePermission(Permissions.Travel.WorkflowHubRead)]
public sealed class WorkflowHubController(IMediator mediator, ITenantContext tenantContext, IFeatureGate featureGate) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.TravelWorkflowHub, tenantContext.TenantId, tenantContext.UserId, cancellationToken);
        var result = await mediator.Send(new ListWorkflowHubQuery(tenantContext.TenantId, page, pageSize), cancellationToken);
        return Ok(result);
    }
}
