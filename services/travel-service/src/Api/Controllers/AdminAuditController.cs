using MediatR;

using Microsoft.AspNetCore.Mvc;

using TravelService.Api.Auth;
using TravelService.Application.Abstractions;

using TravelService.Application.Queries.GetAuditLog;



namespace TravelService.Api.Controllers;



[ApiController]

[Route("admin/audit")]
[RequirePermission(Permissions.Travel.AuditRead)]

public sealed class AdminAuditController(IMediator mediator, ITenantContext tenantContext, IFeatureGate featureGate) : ControllerBase

{

    [HttpGet("{entityType}/{entityId:guid}")]

    public async Task<IActionResult> GetAuditLog(string entityType, Guid entityId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)

    {

        await featureGate.EnsureEnabledAsync(FeatureKeys.TravelAuditRead, tenantContext.TenantId, tenantContext.UserId, cancellationToken);

        var result = await mediator.Send(new GetAuditLogQuery(tenantContext.TenantId, entityType, entityId, page, pageSize), cancellationToken);

        return Ok(result);

    }

}

