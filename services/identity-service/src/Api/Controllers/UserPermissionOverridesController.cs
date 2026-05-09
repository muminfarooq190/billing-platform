using IdentityService.Application.Abstractions;
using IdentityService.Domain.Aggregates;
using IdentityService.Infrastructure.Auth;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("identity/users/{userId:guid}/permission-overrides")]
public sealed class UserPermissionOverridesController(
    IdentityDbContext dbContext,
    IFeatureGate featureGate,
    ITenantContext tenantContext) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.Identity.UsersManage)]
    public async Task<IActionResult> List(Guid userId, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        var overrides = await dbContext.UserPermissionOverrides.AsNoTracking()
            .Where(x => x.UserId == userId && x.TenantId == tenantId)
            .OrderBy(x => x.PermissionKey)
            .ToListAsync(cancellationToken);
        return Ok(overrides.Select(MapResponse));
    }

    [HttpPut]
    [RequirePermission(Permissions.Identity.UsersManage)]
    public async Task<IActionResult> BulkReplace(Guid userId, [FromBody] BulkOverridesRequest? request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest("Body is required.");
        var tenantId = ResolveTenantId();
        await featureGate.EnsureEnabledAsync(FeatureKeys.IdentitySettingsManage, tenantId, tenantContext.UserId, cancellationToken);

        // Verify user exists in tenant.
        var userExists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userId && u.TenantId == tenantId, cancellationToken);
        if (!userExists) return NotFound(new { message = "User not found in tenant." });

        var existing = await dbContext.UserPermissionOverrides
            .Where(x => x.UserId == userId && x.TenantId == tenantId)
            .ToListAsync(cancellationToken);
        dbContext.UserPermissionOverrides.RemoveRange(existing);

        var inserted = new List<UserPermissionOverride>();
        foreach (var entry in request.Overrides ?? new List<OverrideEntry>())
        {
            if (string.IsNullOrWhiteSpace(entry.PermissionKey)) continue;
            inserted.Add(UserPermissionOverride.Create(userId, tenantId, entry.PermissionKey, entry.Granted, entry.Reason));
        }
        dbContext.UserPermissionOverrides.AddRange(inserted);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(inserted.Select(MapResponse));
    }

    [HttpDelete("{permissionKey}")]
    [RequirePermission(Permissions.Identity.UsersManage)]
    public async Task<IActionResult> Remove(Guid userId, string permissionKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(permissionKey)) return BadRequest("Permission key is required.");
        var tenantId = ResolveTenantId();
        var key = permissionKey.Trim();

        var ov = await dbContext.UserPermissionOverrides
            .FirstOrDefaultAsync(x => x.UserId == userId && x.TenantId == tenantId && x.PermissionKey == key, cancellationToken);
        if (ov is null) return NotFound();

        dbContext.UserPermissionOverrides.Remove(ov);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static object MapResponse(UserPermissionOverride o) => new
    {
        id = o.Id,
        userId = o.UserId,
        tenantId = o.TenantId,
        permissionKey = o.PermissionKey,
        granted = o.Granted,
        reason = o.Reason,
        createdAt = o.CreatedAt,
        updatedAt = o.UpdatedAt,
    };

    private Guid ResolveTenantId()
    {
        var raw = User.Claims.FirstOrDefault(x => x.Type == "tenantId" || x.Type == "tenant_id")?.Value
            ?? Request.Headers["X-Tenant-Id"].FirstOrDefault()
            ?? Request.Headers["x-tenant-id"].FirstOrDefault();
        if (!Guid.TryParse(raw, out var tenantId)) throw new InvalidOperationException("Tenant context is missing.");
        return tenantId;
    }

    public sealed record OverrideEntry(string PermissionKey, bool Granted, string? Reason);
    public sealed record BulkOverridesRequest(List<OverrideEntry>? Overrides);
}
