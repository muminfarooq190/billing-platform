using IdentityService.Api.Contracts;
using IdentityService.Application.Abstractions;
using IdentityService.Domain.Aggregates;
using IdentityService.Infrastructure.Auth;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("identity/tenant-settings")]
public sealed class TenantSettingsController(IdentityDbContext dbContext, IFeatureGate featureGate, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.Identity.SettingsRead)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        await featureGate.EnsureEnabledAsync(FeatureKeys.IdentitySettingsManage, tenantId, tenantContext.UserId, cancellationToken);
        var settings = await dbContext.TenantSettings.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        return settings is null ? NotFound() : Ok(settings);
    }

    [HttpPut]
    [RequirePermission(Permissions.Identity.SettingsManage)]
    public async Task<IActionResult> Put([FromBody] UpdateTenantSettingsRequest request, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        await featureGate.EnsureEnabledAsync(FeatureKeys.IdentitySettingsManage, tenantId, tenantContext.UserId, cancellationToken);
        var settings = await dbContext.TenantSettings.FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        if (settings is null)
        {
            settings = TenantSettings.Create(tenantId, request.Timezone, request.Locale, request.DateFormat, request.Currency, request.NumberFormat, request.DefaultCountry, request.SettingsJson);
            dbContext.TenantSettings.Add(settings);
        }
        else
        {
            settings.Update(request.Timezone, request.Locale, request.DateFormat, request.Currency, request.NumberFormat, request.DefaultCountry, request.SettingsJson);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(settings);
    }

    private Guid ResolveTenantId()
    {
        var raw = User.Claims.FirstOrDefault(x => x.Type == "tenantId" || x.Type == "tenant_id")?.Value
            ?? Request.Headers["X-Tenant-Id"].FirstOrDefault()
            ?? Request.Headers["x-tenant-id"].FirstOrDefault();
        if (!Guid.TryParse(raw, out var tenantId)) throw new InvalidOperationException("Tenant context is missing.");
        return tenantId;
    }
}
