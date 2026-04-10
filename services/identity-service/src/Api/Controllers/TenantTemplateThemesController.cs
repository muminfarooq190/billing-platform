using IdentityService.Api.Contracts;
using IdentityService.Domain.Aggregates;
using IdentityService.Infrastructure.Auth;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("tenant-branding/templates")]
[RequirePermission(Permissions.Branding.ThemeManage)]
public sealed class TenantTemplateThemesController(IdentityDbContext dbContext) : ControllerBase
{
    [HttpGet("{scope}")]
    public async Task<IActionResult> Get(string scope, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        var theme = await dbContext.TenantTemplateThemes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.TemplateScope == scope, cancellationToken);

        return theme is null
            ? NotFound()
            : Ok(new TenantTemplateThemeResponse(theme.Id, theme.TemplateScope, theme.HeaderHtml, theme.FooterHtml, theme.CustomCss, theme.LogoAssetId, theme.BackgroundAssetId, theme.SettingsJson, theme.CreatedAt, theme.UpdatedAt));
    }

    [HttpPut("{scope}")]
    public async Task<IActionResult> Put(string scope, [FromBody] UpdateTenantTemplateThemeRequest request, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        var theme = await dbContext.TenantTemplateThemes
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.TemplateScope == scope, cancellationToken);

        if (theme is null)
        {
            theme = TenantTemplateTheme.Create(tenantId, scope);
            theme.Update(request.HeaderHtml, request.FooterHtml, request.CustomCss, request.LogoAssetId, request.BackgroundAssetId, request.SettingsJson);
            dbContext.TenantTemplateThemes.Add(theme);
        }
        else
        {
            theme.Update(request.HeaderHtml, request.FooterHtml, request.CustomCss, request.LogoAssetId, request.BackgroundAssetId, request.SettingsJson);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new TenantTemplateThemeResponse(theme.Id, theme.TemplateScope, theme.HeaderHtml, theme.FooterHtml, theme.CustomCss, theme.LogoAssetId, theme.BackgroundAssetId, theme.SettingsJson, theme.CreatedAt, theme.UpdatedAt));
    }

    private Guid ResolveTenantId()
    {
        var raw = User.Claims.FirstOrDefault(x => x.Type == "tenantId")?.Value
            ?? Request.Headers["X-Tenant-Id"].FirstOrDefault();

        if (!Guid.TryParse(raw, out var tenantId))
            throw new InvalidOperationException("Tenant context is missing.");

        return tenantId;
    }
}
