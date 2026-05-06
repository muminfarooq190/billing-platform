using BillingService.Api.Contracts;
using BillingService.Application.Abstractions;
using BillingService.Application.Commands.AssignTenantPackage;
using BillingService.Application.Commands.CreateTenantFeatureOverride;
using BillingService.Application.Commands.GrantFeatureEntitlement;
using BillingService.Application.Queries.GetEffectiveEntitlements;
using BillingService.Application.ReadModels;
using Dapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("billing/entitlements")]
public sealed class EntitlementsController(IMediator mediator, ITenantContext tenantContext, IReadDbConnectionFactory readDbConnectionFactory) : ControllerBase
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

        try
        {
            var model = await mediator.Send(new GetEffectiveEntitlementsQuery(tenantId), cancellationToken);
            return Ok(model);
        }
        catch
        {
            const string sql = @"
select distinct on (feature_key)
    feature_key as ""FeatureKey"",
    granted as ""Granted"",
    source as ""Source"",
    plan_type as ""PlanType"",
    limit_value as ""LimitValue"",
    effective_from as ""EffectiveFrom"",
    effective_to as ""EffectiveTo"",
    metadata_json as ""MetadataJson""
from (
    select
        cpf.feature_key,
        cpf.granted,
        tsp.source,
        cp.plan_type,
        cpf.limit_value,
        tsp.effective_from,
        tsp.effective_to,
        tsp.metadata_json
    from tenant_subscription_packages tsp
    join commercial_packages cp on cp.""Id"" = tsp.commercial_package_id
    join commercial_package_features cpf on cpf.commercial_package_id = cp.""Id""
    where tsp.tenant_id = @tenantId
      and tsp.deleted_at is null
      and (tsp.effective_to is null or tsp.effective_to >= now())

    union all

    select
        feature_key,
        granted,
        source,
        null as plan_type,
        limit_value,
        effective_from,
        effective_to,
        metadata_json
    from tenant_feature_overrides
    where tenant_id = @tenantId
      and deleted_at is null
      and (effective_to is null or effective_to >= now())
) entitlements
order by feature_key, effective_from desc nulls last;";

            using var connection = await readDbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
            var model = (await connection.QueryAsync<FeatureEntitlementReadModel>(sql, new { tenantId })).ToList();
            return Ok(model);
        }
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
