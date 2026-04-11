using IdentityService.Api.Contracts;
using IdentityService.Application.Abstractions;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.Exceptions;
using IdentityService.Infrastructure.Auth;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("identity")]
[RequirePermission(Permissions.Identity.RolesManage)]
public sealed class RolesController(IdentityDbContext dbContext, IFeatureGate featureGate) : ControllerBase
{
    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions(CancellationToken cancellationToken)
        => Ok(await dbContext.PermissionDefinitions.AsNoTracking().OrderBy(x => x.Category).ThenBy(x => x.Key).ToListAsync(cancellationToken));

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        await featureGate.EnsureEnabledAsync(FeatureKeys.IdentityRbacAdvanced, tenantId, cancellationToken);
        var roles = await dbContext.RoleDefinitions.AsNoTracking()
            .Where(x => x.TenantId == null || x.TenantId == tenantId)
            .OrderByDescending(x => x.IsSystemDefault)
            .ThenBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.TenantId,
                x.Name,
                x.Description,
                x.IsSystemDefault,
                PermissionKeys = dbContext.RolePermissionAssignments.Where(p => p.RoleDefinitionId == x.Id).Select(p => p.PermissionKey).ToList()
            })
            .ToListAsync(cancellationToken);
        return Ok(roles);
    }

    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        await featureGate.EnsureEnabledAsync(FeatureKeys.IdentityRbacAdvanced, tenantId, cancellationToken);
        var normalized = request.Name.Trim().ToUpperInvariant();
        var exists = await dbContext.RoleDefinitions.AnyAsync(x => x.TenantId == tenantId && x.NormalizedName == normalized, cancellationToken);
        if (exists) throw new ConflictException("Role already exists.");

        var role = RoleDefinition.Create(tenantId, request.Name, request.Description);
        role.SetPermissions(request.PermissionKeys);
        dbContext.RoleDefinitions.Add(role);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { role.Id, role.Name, role.Description });
    }

    [HttpPut("roles/{id:guid}")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        await featureGate.EnsureEnabledAsync(FeatureKeys.IdentityRbacAdvanced, tenantId, cancellationToken);
        var role = await dbContext.RoleDefinitions.Include(x => x.Permissions).FirstOrDefaultAsync(x => x.Id == id && (x.TenantId == tenantId || x.TenantId == null), cancellationToken)
            ?? throw new NotFoundException("Role not found.");
        role.Update(request.Name, request.Description);
        role.SetPermissions(request.PermissionKeys);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { role.Id, role.Name, role.Description });
    }

    [HttpPut("users/{userId:guid}/roles")]
    public async Task<IActionResult> UpdateUserRoles(Guid userId, [FromBody] UpdateUserRolesRequest request, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        await featureGate.EnsureEnabledAsync(FeatureKeys.IdentityRbacAdvanced, tenantId, cancellationToken);
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && x.DeletedAt == null, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var validRoleIds = await dbContext.RoleDefinitions.Where(x => request.RoleIds.Contains(x.Id) && (x.TenantId == tenantId || x.TenantId == null)).Select(x => x.Id).ToListAsync(cancellationToken);
        if (validRoleIds.Count != request.RoleIds.Count) throw new DomainException("One or more role ids are invalid.");

        var existing = await dbContext.UserRoleAssignments.Where(x => x.TenantId == tenantId && x.UserId == userId).ToListAsync(cancellationToken);
        dbContext.UserRoleAssignments.RemoveRange(existing);
        foreach (var roleId in validRoleIds)
        {
            dbContext.UserRoleAssignments.Add(UserRoleAssignment.Create(tenantId, userId, roleId));
        }

        dbContext.IdentityAuditLogs.Add(IdentityAuditLog.Create(tenantId, ResolveActorUserId(), userId, "UserRolesUpdated", null, $"{{\"roleCount\":{validRoleIds.Count}}}", HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString()));
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private Guid ResolveTenantId()
    {
        var raw = User.Claims.FirstOrDefault(x => x.Type == "tenantId" || x.Type == "tenant_id")?.Value ?? Request.Headers["X-Tenant-Id"].FirstOrDefault();
        if (!Guid.TryParse(raw, out var tenantId)) throw new InvalidOperationException("Tenant context is missing.");
        return tenantId;
    }

    private Guid ResolveActorUserId()
    {
        var raw = User.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
        return Guid.TryParse(raw, out var userId) ? userId : Guid.Empty;
    }
}
