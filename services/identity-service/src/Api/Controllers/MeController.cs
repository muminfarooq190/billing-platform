using System.Security.Claims;
using IdentityService.Api.Contracts;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Controllers;

[ApiController]
[Authorize]
[Route("identity/me")]
public sealed class MeController(ITenantContext tenantContext, IdentityDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IdentityMeResponse>> Get(CancellationToken cancellationToken)
    {
        var userId = tenantContext.UserId ?? ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var tenantId = tenantContext.TenantId;

        var user = await dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && x.DeletedAt == null, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        var tenantName = await dbContext.Tenants.AsNoTracking()
            .Where(x => x.Id == tenantId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);

        var permissions = User.Claims
            .Where(x => x.Type == "permissions")
            .SelectMany(x => x.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToArray();

        var roleKeys = User.Claims
            .Where(x => x.Type == ClaimTypes.Role || x.Type == "role")
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToArray();

        var displayName = BuildDisplayName(user.Email);

        return Ok(new IdentityMeResponse(
            user.Id,
            user.Email,
            displayName,
            string.Equals(user.Status.ToString(), "Active", StringComparison.OrdinalIgnoreCase),
            tenantId,
            tenantName,
            roleKeys,
            permissions
        ));
    }

    private Guid ResolveUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(raw, out var userId) ? userId : Guid.Empty;
    }

    private static string BuildDisplayName(string email)
    {
        return email;
    }
}
