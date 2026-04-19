using BillingService.Api.Contracts;
using BillingService.Application.Abstractions;
using BillingService.Application.Queries.GetEffectiveEntitlements;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("billing/tenants/{tenantId:guid}")]
public sealed class TenantBillingController(
    ITenantSubscriptionPackageRepository tenantSubscriptionPackageRepository,
    ITenantFeatureOverrideRepository tenantFeatureOverrideRepository,
    ICommercialPackageRepository commercialPackageRepository,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    ITenantContext tenantContext) : ControllerBase
{
    [HttpGet("packages")]
    public async Task<IActionResult> ListPackages(Guid tenantId, CancellationToken cancellationToken)
    {
        if (tenantId != tenantContext.TenantId)
            return Forbid();

        var assignments = await tenantSubscriptionPackageRepository.ListByTenantIdAsync(tenantId, cancellationToken);
        return Ok(assignments);
    }

    [HttpPost("packages")]
    public async Task<IActionResult> CreatePackage(Guid tenantId, [FromBody] AssignTenantPackageRequest request, CancellationToken cancellationToken)
    {
        if (tenantId != tenantContext.TenantId)
            return Forbid();
        var package = await commercialPackageRepository.GetByIdAsync(request.CommercialPackageId, cancellationToken);
        if (package is null)
            return NotFound(new ProblemDetails { Status = 404, Detail = $"Commercial package '{request.CommercialPackageId}' was not found." });

        var assignment = TenantSubscriptionPackage.Create(tenantId, request.CommercialPackageId, request.Source, request.Status, request.EffectiveFrom, request.EffectiveTo, request.MetadataJson);
        await tenantSubscriptionPackageRepository.AddAsync(assignment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(ListPackages), new { tenantId }, assignment);
    }

    [HttpPut("packages/{assignmentId:guid}")]
    public async Task<IActionResult> UpdatePackage(Guid tenantId, Guid assignmentId, [FromBody] UpdateTenantPackageRequest request, CancellationToken cancellationToken)
    {
        if (tenantId != tenantContext.TenantId)
            return Forbid();
        var assignment = await tenantSubscriptionPackageRepository.GetByIdAsync(assignmentId, cancellationToken);
        if (assignment is null || assignment.TenantId != tenantId)
            return NotFound();

        var package = await commercialPackageRepository.GetByIdAsync(request.CommercialPackageId, cancellationToken);
        if (package is null)
            return NotFound(new ProblemDetails { Status = 404, Detail = $"Commercial package '{request.CommercialPackageId}' was not found." });

        assignment.Update(request.CommercialPackageId, request.Source, request.Status, request.EffectiveFrom, request.EffectiveTo, request.MetadataJson);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(assignment);
    }

    [HttpDelete("packages/{assignmentId:guid}")]
    public async Task<IActionResult> DeletePackage(Guid tenantId, Guid assignmentId, CancellationToken cancellationToken)
    {
        if (tenantId != tenantContext.TenantId)
            return Forbid();
        var assignment = await tenantSubscriptionPackageRepository.GetByIdAsync(assignmentId, cancellationToken);
        if (assignment is null || assignment.TenantId != tenantId)
            return NotFound();

        assignment.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("feature-overrides")]
    public async Task<IActionResult> ListFeatureOverrides(Guid tenantId, CancellationToken cancellationToken)
    {
        if (tenantId != tenantContext.TenantId)
            return Forbid();

        var overrides = await tenantFeatureOverrideRepository.ListByTenantIdAsync(tenantId, cancellationToken);
        return Ok(overrides);
    }

    [HttpPost("feature-overrides")]
    public async Task<IActionResult> CreateFeatureOverride(Guid tenantId, [FromBody] CreateTenantFeatureOverrideRequest request, CancellationToken cancellationToken)
    {
        if (tenantId != tenantContext.TenantId)
            return Forbid();
        var entry = TenantFeatureOverride.Create(tenantId, request.FeatureKey, request.Granted, request.Reason, request.Source, request.LimitValue, request.CreatedBy, request.EffectiveFrom, request.EffectiveTo, request.MetadataJson);
        await tenantFeatureOverrideRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(ListFeatureOverrides), new { tenantId }, entry);
    }

    [HttpPut("feature-overrides/{overrideId:guid}")]
    public async Task<IActionResult> UpdateFeatureOverride(Guid tenantId, Guid overrideId, [FromBody] UpdateTenantFeatureOverrideRequest request, CancellationToken cancellationToken)
    {
        if (tenantId != tenantContext.TenantId)
            return Forbid();
        var entry = await tenantFeatureOverrideRepository.GetByIdAsync(overrideId, cancellationToken);
        if (entry is null || entry.TenantId != tenantId)
            return NotFound();

        entry.Update(request.FeatureKey, request.Granted, request.LimitValue, request.Reason, request.Source, request.CreatedBy, request.EffectiveFrom, request.EffectiveTo, request.MetadataJson);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(entry);
    }

    [HttpDelete("feature-overrides/{overrideId:guid}")]
    public async Task<IActionResult> DeleteFeatureOverride(Guid tenantId, Guid overrideId, CancellationToken cancellationToken)
    {
        if (tenantId != tenantContext.TenantId)
            return Forbid();
        var entry = await tenantFeatureOverrideRepository.GetByIdAsync(overrideId, cancellationToken);
        if (entry is null || entry.TenantId != tenantId)
            return NotFound();

        entry.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("entitlements")]
    public async Task<IActionResult> GetEntitlements(Guid tenantId, CancellationToken cancellationToken)
    {
        if (tenantId != tenantContext.TenantId)
            return Forbid();

        return Ok(await mediator.Send(new GetEffectiveEntitlementsQuery(tenantId), cancellationToken));
    }

    [HttpGet("entitlements/{featureKey}")]
    public async Task<IActionResult> GetEntitlement(Guid tenantId, string featureKey, CancellationToken cancellationToken)
    {
        if (tenantId != tenantContext.TenantId)
            return Forbid();

        var items = await mediator.Send(new GetEffectiveEntitlementsQuery(tenantId), cancellationToken);
        var entry = items.FirstOrDefault(x => string.Equals(x.FeatureKey, featureKey, StringComparison.OrdinalIgnoreCase));
        return entry is null ? NotFound() : Ok(entry);
    }
}
