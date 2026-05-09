using IdentityService.Application.Abstractions;
using IdentityService.Domain.Aggregates;
using IdentityService.Infrastructure.Auth;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Controllers;

/// <summary>
/// Tenant-specific PDF styling + email template styling. Read+upsert per tenant.
/// </summary>
[ApiController]
[Route("tenant-branding")]
public sealed class TenantStylingController(
    IdentityDbContext dbContext,
    IFeatureGate featureGate,
    ITenantContext tenantContext) : ControllerBase
{
    [HttpGet("pdf-styling")]
    [RequirePermission(Permissions.Branding.ThemeRead)]
    public async Task<IActionResult> GetPdfStyling(CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        var styling = await dbContext.TenantPdfStylings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        return Ok(styling ?? TenantPdfStyling.Create(tenantId, null, null, null, null, null, null));
    }

    [HttpPut("pdf-styling")]
    [RequirePermission(Permissions.Branding.ThemeManage)]
    public async Task<IActionResult> UpsertPdfStyling([FromBody] UpsertPdfStylingRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest("Body is required.");
        var tenantId = ResolveTenantId();
        await featureGate.EnsureEnabledAsync(FeatureKeys.IdentitySettingsManage, tenantId, tenantContext.UserId, cancellationToken);

        var styling = await dbContext.TenantPdfStylings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        if (styling is null)
        {
            styling = TenantPdfStyling.Create(tenantId, request.HeaderLayout, request.FooterText, request.WatermarkText, request.AccentColor, request.MarginPx, request.CustomCssJson);
            dbContext.TenantPdfStylings.Add(styling);
        }
        else
        {
            styling.Update(request.HeaderLayout, request.FooterText, request.WatermarkText, request.AccentColor, request.MarginPx, request.CustomCssJson);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(styling);
    }

    [HttpGet("email-templates")]
    [RequirePermission(Permissions.Branding.ThemeRead)]
    public async Task<IActionResult> GetEmailTemplateStyle(CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        var style = await dbContext.TenantEmailTemplateStyles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        return Ok(style ?? TenantEmailTemplateStyle.Create(tenantId, null, null, null, null, null));
    }

    [HttpPut("email-templates")]
    [RequirePermission(Permissions.Branding.ThemeManage)]
    public async Task<IActionResult> UpsertEmailTemplateStyle([FromBody] UpsertEmailTemplateStyleRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest("Body is required.");
        var tenantId = ResolveTenantId();
        await featureGate.EnsureEnabledAsync(FeatureKeys.IdentitySettingsManage, tenantId, tenantContext.UserId, cancellationToken);

        var style = await dbContext.TenantEmailTemplateStyles
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        if (style is null)
        {
            style = TenantEmailTemplateStyle.Create(tenantId, request.HeaderHtml, request.FooterHtml, request.AccentColor, request.FontFamily, request.CustomCssJson);
            dbContext.TenantEmailTemplateStyles.Add(style);
        }
        else
        {
            style.Update(request.HeaderHtml, request.FooterHtml, request.AccentColor, request.FontFamily, request.CustomCssJson);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(style);
    }

    private Guid ResolveTenantId()
    {
        var raw = User.Claims.FirstOrDefault(x => x.Type == "tenantId" || x.Type == "tenant_id")?.Value
            ?? Request.Headers["X-Tenant-Id"].FirstOrDefault()
            ?? Request.Headers["x-tenant-id"].FirstOrDefault();
        if (!Guid.TryParse(raw, out var tenantId)) throw new InvalidOperationException("Tenant context is missing.");
        return tenantId;
    }

    public sealed record UpsertPdfStylingRequest(
        string? HeaderLayout,
        string? FooterText,
        string? WatermarkText,
        string? AccentColor,
        int? MarginPx,
        string? CustomCssJson);

    public sealed record UpsertEmailTemplateStyleRequest(
        string? HeaderHtml,
        string? FooterHtml,
        string? AccentColor,
        string? FontFamily,
        string? CustomCssJson);
}
