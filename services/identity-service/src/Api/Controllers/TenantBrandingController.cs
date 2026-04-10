using IdentityService.Api.Contracts;
using IdentityService.Application.Abstractions;
using IdentityService.Domain.Aggregates;
using IdentityService.Infrastructure.Auth;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("tenant-branding")]
[RequirePermission("branding.theme.manage")]
public sealed class TenantBrandingController(IdentityDbContext dbContext, IBrandAssetStorage storage) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        var branding = await dbContext.TenantBranding.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        return branding is null ? NotFound() : Ok(branding);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] UpdateTenantBrandingRequest request, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        var branding = await dbContext.TenantBranding.FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (branding is null)
        {
            branding = TenantBranding.Create(tenantId, request.DisplayName);
            branding.Update(request.DisplayName, request.LegalName, request.PrimaryColor, request.SecondaryColor, request.AccentColor, request.TextColor, request.BackgroundColor, request.ThemeMode, request.DefaultFontFamily, request.SupportEmail, request.SupportPhone, request.WebsiteUrl, request.Tagline);
            dbContext.TenantBranding.Add(branding);
        }
        else
        {
            branding.Update(request.DisplayName, request.LegalName, request.PrimaryColor, request.SecondaryColor, request.AccentColor, request.TextColor, request.BackgroundColor, request.ThemeMode, request.DefaultFontFamily, request.SupportEmail, request.SupportPhone, request.WebsiteUrl, request.Tagline);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(branding);
    }

    [HttpGet("assets")]
    public async Task<IActionResult> GetAssets(CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        var assets = await dbContext.TenantBrandAssets
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.DeletedAt == null)
            .OrderBy(x => x.AssetType)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new TenantBrandingAssetResponse(x.Id, x.AssetType, x.StorageKey, x.OriginalFileName, x.ContentType, x.SizeBytes, x.Width, x.Height, x.AltText, x.IsActive, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Ok(assets);
    }

    [HttpPost("assets")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadAsset([FromForm] IFormFile file, [FromForm] string assetType, [FromForm] string? altText, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        if (file is null || file.Length == 0)
        {
            return BadRequest("File is required.");
        }

        var allowedTypes = new[] { "image/png", "image/jpeg", "image/webp", "image/svg+xml", "image/x-icon", "image/vnd.microsoft.icon" };
        if (!allowedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest("Unsupported file type.");
        }

        if (file.Length > 10_000_000)
        {
            return BadRequest("File is too large.");
        }

        var safeFileName = Path.GetFileName(file.FileName);
        var storageKey = $"tenant/{tenantId}/branding/{assetType.ToLowerInvariant()}/{Guid.NewGuid()}-{safeFileName}";

        await using var stream = file.OpenReadStream();
        await storage.SaveAsync(storageKey, stream, cancellationToken);

        var asset = TenantBrandAsset.Create(tenantId, assetType, storageKey, safeFileName, file.ContentType, file.Length, null, null, altText);
        dbContext.TenantBrandAssets.Add(asset);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new TenantBrandingAssetResponse(asset.Id, asset.AssetType, asset.StorageKey, asset.OriginalFileName, asset.ContentType, asset.SizeBytes, asset.Width, asset.Height, asset.AltText, asset.IsActive, asset.CreatedAt, asset.UpdatedAt));
    }

    [HttpDelete("assets/{assetId:guid}")]
    public async Task<IActionResult> DeleteAsset(Guid assetId, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        var asset = await dbContext.TenantBrandAssets.FirstOrDefaultAsync(x => x.Id == assetId && x.TenantId == tenantId, cancellationToken);
        if (asset is null)
        {
            return NotFound();
        }

        asset.SoftDelete();
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private Guid ResolveTenantId()
    {
        var raw = User.Claims.FirstOrDefault(x => x.Type == "tenantId")?.Value
            ?? Request.Headers["X-Tenant-Id"].FirstOrDefault();

        if (!Guid.TryParse(raw, out var tenantId))
        {
            throw new InvalidOperationException("Tenant context is missing.");
        }

        return tenantId;
    }
}
