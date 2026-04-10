using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("identity")]
[Authorize(Roles = "Admin,Owner")]
public sealed class IdentityAuditController(IdentityDbContext dbContext) : ControllerBase
{
    [HttpGet("audit/users/{userId:guid}")]
    public async Task<IActionResult> GetAudit(Guid userId, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        var items = await dbContext.IdentityAuditLogs.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TargetUserId == userId)
            .OrderByDescending(x => x.OccurredAt)
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("security-events")]
    public async Task<IActionResult> GetSecurityEvents([FromQuery] Guid? userId, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        var query = dbContext.SecurityEvents.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId);
        }

        var items = await query.OrderByDescending(x => x.OccurredAt).Take(200).ToListAsync(cancellationToken);
        return Ok(items);
    }

    private Guid ResolveTenantId()
    {
        var raw = User.Claims.FirstOrDefault(x => x.Type == "tenantId")?.Value ?? Request.Headers["X-Tenant-Id"].FirstOrDefault();
        if (!Guid.TryParse(raw, out var tenantId)) throw new InvalidOperationException("Tenant context is missing.");
        return tenantId;
    }
}
