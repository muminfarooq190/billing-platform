using System.Text;
using IdentityService.Application.Abstractions;
using IdentityService.Infrastructure.Auth;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("identity")]
[RequirePermission(Permissions.Identity.AuditRead)]
public sealed class IdentityAuditController(IdentityDbContext dbContext, IFeatureGate featureGate) : ControllerBase
{
    [HttpGet("audit/users/{userId:guid}")]
    public async Task<IActionResult> GetAudit(Guid userId, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        await featureGate.EnsureEnabledAsync(FeatureKeys.IdentityAuditRead, tenantId, cancellationToken);
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
        await featureGate.EnsureEnabledAsync(FeatureKeys.IdentityAuditRead, tenantId, cancellationToken);
        var query = dbContext.SecurityEvents.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId);
        }

        var items = await query.OrderByDescending(x => x.OccurredAt).Take(200).ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("audit/export")]
    public async Task<IActionResult> ExportAudit([FromQuery] Guid? userId, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        await featureGate.EnsureEnabledAsync(FeatureKeys.IdentityAuditExport, tenantId, cancellationToken);

        var query = dbContext.IdentityAuditLogs.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (userId.HasValue)
        {
            query = query.Where(x => x.TargetUserId == userId);
        }

        var items = await query
            .OrderByDescending(x => x.OccurredAt)
            .Take(1000)
            .ToListAsync(cancellationToken);

        var csv = new StringBuilder();
        csv.AppendLine("OccurredAt,EventType,ActorUserId,TargetUserId,IpAddress,UserAgent,BeforeJson,AfterJson");
        foreach (var item in items)
        {
            csv.Append('"').Append(item.OccurredAt.ToString("O")).Append("\",");
            csv.Append('"').Append(Escape(item.EventType)).Append("\",");
            csv.Append('"').Append(item.ActorUserId?.ToString() ?? string.Empty).Append("\",");
            csv.Append('"').Append(item.TargetUserId?.ToString() ?? string.Empty).Append("\",");
            csv.Append('"').Append(Escape(item.IpAddress)).Append("\",");
            csv.Append('"').Append(Escape(item.UserAgent)).Append("\",");
            csv.Append('"').Append(Escape(item.BeforeJson)).Append("\",");
            csv.Append('"').Append(Escape(item.AfterJson)).AppendLine("\"");
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"identity-audit-{tenantId:N}.csv");
    }

    private static string Escape(string? value)
        => (value ?? string.Empty).Replace("\"", "\"\"");

    private Guid ResolveTenantId()
    {
        var raw = User.Claims.FirstOrDefault(x => x.Type == "tenantId")?.Value ?? Request.Headers["X-Tenant-Id"].FirstOrDefault();
        if (!Guid.TryParse(raw, out var tenantId)) throw new InvalidOperationException("Tenant context is missing.");
        return tenantId;
    }
}
