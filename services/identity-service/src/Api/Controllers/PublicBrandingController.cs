using IdentityService.Api.Contracts;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("identity/internal/public-branding")]
public sealed class PublicBrandingController(IdentityDbContext dbContext) : ControllerBase
{
    [HttpGet("{tenantId:guid}")]
    public async Task<ActionResult<PublicBrandingResponse>> Get(Guid tenantId, CancellationToken cancellationToken)
    {
        var branding = await dbContext.TenantBranding.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (branding is null)
        {
            return NotFound();
        }

        var assets = await dbContext.TenantBrandAssets.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.DeletedAt == null && x.IsActive)
            .OrderBy(x => x.AssetType)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new PublicBrandingAssetResponse(
                x.Id,
                x.AssetType,
                x.OriginalFileName,
                x.ContentType,
                x.AltText,
                x.IsActive,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(new PublicBrandingResponse(
            branding.DisplayName,
            branding.Tagline,
            branding.PrimaryColor,
            branding.SecondaryColor,
            branding.AccentColor,
            branding.TextColor,
            branding.BackgroundColor,
            branding.ThemeMode,
            branding.DefaultFontFamily,
            branding.SupportEmail,
            branding.SupportPhone,
            branding.WebsiteUrl,
            assets
        ));
    }
}
