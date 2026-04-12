using BillingService.Api.Contracts;
using BillingService.Application.Commands.AssignUserFeatures;
using BillingService.Application.Commands.RevokeUserFeatureAssignment;
using BillingService.Application.Queries.GetMyFeatureAccess;
using BillingService.Application.Queries.GetTenantFeatureAllocations;
using BillingService.Application.Queries.GetUserFeatureAccess;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("billing")]
public sealed class UserFeatureAssignmentsController(IMediator mediator, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet("tenants/{tenantId:guid}/feature-allocations")]
    public async Task<IActionResult> GetTenantFeatureAllocations(Guid tenantId, CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetTenantFeatureAllocationsQuery(tenantId), cancellationToken));

    [HttpGet("tenants/{tenantId:guid}/users/{userId:guid}/features")]
    public async Task<IActionResult> GetUserFeatures(Guid tenantId, Guid userId, CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetUserFeatureAccessQuery(tenantId, userId), cancellationToken));

    [HttpPost("tenants/{tenantId:guid}/users/{userId:guid}/feature-assignments")]
    public async Task<IActionResult> AssignFeatures(Guid tenantId, Guid userId, [FromBody] AssignUserFeaturesRequest request, CancellationToken cancellationToken)
    {
        var assignedFeatures = await mediator.Send(new AssignUserFeaturesCommand(
            tenantId,
            userId,
            request.FeatureKeys,
            request.AssignedByUserId ?? tenantContext.UserId,
            request.EffectiveFrom,
            request.EffectiveTo,
            request.Notes,
            request.MetadataJson), cancellationToken);

        return Ok(new { assignedFeatures });
    }

    [HttpDelete("tenants/{tenantId:guid}/users/{userId:guid}/feature-assignments/{featureKey}")]
    public async Task<IActionResult> RevokeFeature(Guid tenantId, Guid userId, string featureKey, CancellationToken cancellationToken)
    {
        await mediator.Send(new RevokeUserFeatureAssignmentCommand(tenantId, userId, featureKey, tenantContext.UserId), cancellationToken);
        return NoContent();
    }

    [HttpGet("feature-access/me")]
    public async Task<IActionResult> GetMyFeatureAccess(CancellationToken cancellationToken)
    {
        var userId = tenantContext.UserId ?? throw new InvalidOperationException("Missing authenticated user id.");
        return Ok(await mediator.Send(new GetMyFeatureAccessQuery(tenantContext.TenantId, userId), cancellationToken));
    }
}
