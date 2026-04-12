using IdentityService.Api.Contracts;
using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands.SuspendTenant;
using IdentityService.Application.Queries.GetTenantById;
using IdentityService.Domain.Exceptions;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("tenants")]
[RequirePermission(Permissions.Identity.TenantManage)]
public sealed class TenantsController(IMediator mediator, ITenantRepository tenantRepository, IFeatureGate featureGate, Application.Abstractions.IUnitOfWork unitOfWork, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.IdentityTenantManage, id, tenantContext.UserId, cancellationToken);
        var tenant = await mediator.Send(new GetTenantByIdQuery(id), cancellationToken);
        return tenant is null ? NotFound() : Ok(tenant);
    }

    [HttpPatch("{id:guid}/plan")]
    public async Task<IActionResult> ChangePlan(Guid id, [FromBody] ChangePlanRequest request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.IdentityTenantManage, id, tenantContext.UserId, cancellationToken);
        _ = await tenantRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Tenant not found.");

        return Conflict(new
        {
            code = "billing_owned_plan_model",
            message = "Tenant plan changes are now owned by billing-service package and entitlement resolution. Update billing subscriptions/packages instead of identity tenant plan.",
            requestedPlan = request.Plan
        });
    }

    [HttpPost("{id:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.IdentityTenantManage, id, tenantContext.UserId, cancellationToken);
        await mediator.Send(new SuspendTenantCommand(id), cancellationToken);
        return NoContent();
    }
}
