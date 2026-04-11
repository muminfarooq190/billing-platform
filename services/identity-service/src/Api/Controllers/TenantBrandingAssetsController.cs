using IdentityService.Application.Abstractions;
using IdentityService.Infrastructure.Auth;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("tenant-branding/files")]
[RequirePermission(Permissions.Branding.ThemeManage)]
public sealed class TenantBrandingAssetsController(IdentityDbContext dbContext, IBrandAssetStorage storage, IFeatureGate featureGate) : ControllerBase
{
    [HttpGet("{**storageKey}")]
    public async Task<IActionResult> Read(string storageKey, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        await featureGate.EnsureEnabledAsync(FeatureKeys.BrandingAssetsManage, tenantId, cancellationToken);
        var asset = await dbContext.TenantBrandAssets.AsNoTracking()
            .SingleOrDefaultAsync(x => x.StorageKey == storageKey && x.TenantId == tenantId && x.DeletedAt == null, cancellationToken);

        if (asset is null)
        {
            return NotFound();
        }

        var fullPath = storage.GetAbsolutePath(storageKey);
        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound();
        }

        return PhysicalFile(fullPath, asset.ContentType, asset.OriginalFileName, enableRangeProcessing: true);
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
