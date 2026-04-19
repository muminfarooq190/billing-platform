using BillingService.Api.Contracts;
using BillingService.Application.Commands.AssignTenantPackage;
using BillingService.Application.Commands.CreateTenantFeatureOverride;
using BillingService.Application.Commands.GrantFeatureEntitlement;
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
        if (tenantId != tenantContext.TenantId)
            return Forbid();

        var model = await mediator.Send(new GetEffectiveEntitlementsQuery(tenantId), cancellationToken);
        return Ok(model);
    }

    [HttpPost("{tenantId:guid}/grants")]
    public async Task<IActionResult> Grant(Guid tenantId, [FromBody] GrantFeatureEntitlementRequest request, CancellationToken cancellationToken)
    {
        if (tenantId != tenantContext.TenantId)
            return Forbid();

        var result = await mediator.Send(new GrantFeatureEntitlementCommand(tenantId, request.FeatureKey, request.Granted, request.LimitValue, request.EffectiveFrom, request.EffectiveTo, request.Reason), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{tenantId:guid}/packages")]
    public async Task<IActionResult> AssignPackage(Guid tenantId, [FromBody] AssignTenantPackageRequest request, CancellationToken cancellationToken)
    {
        if (tenantId != tenantContext.TenantId)
            return Forbid();
        var id = await mediator.Send(new AssignTenantPackageCommand(
            tenantId,
            request.CommercialPackageId,
            request.Source,
            request.Status,
            request.EffectiveFrom,
            request.EffectiveTo,
            request.MetadataJson), cancellationToken);

        return Ok(new { Id = id });
    }

    [HttpPost("{tenantId:guid}/overrides")]
    public async Task<IActionResult> CreateOverride(Guid tenantId, [FromBody] CreateTenantFeatureOverrideRequest request, CancellationToken cancellationToken)
    {
        if (tenantId != tenantContext.TenantId)
            return Forbid();
        var id = await mediator.Send(new CreateTenantFeatureOverrideCommand(
            tenantId,
            request.FeatureKey,
            request.Granted,
            request.LimitValue,
            request.Reason,
            request.Source,
            request.CreatedBy,
            request.EffectiveFrom,
            request.EffectiveTo,
            request.MetadataJson), cancellationToken);

        return Ok(new { Id = id });
    }
}
