using IdentityService.Api.Contracts;
using IdentityService.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("identity")]
[Authorize]
public sealed class SessionsController(RefreshTokenService refreshTokenService) : ControllerBase
{
    [HttpGet("me/sessions")]
    public async Task<IActionResult> ListMine(CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        var sessions = await refreshTokenService.ListSessionsAsync(userId, cancellationToken);
        return Ok(sessions);
    }

    [HttpDelete("me/sessions/{sessionId:guid}")]
    public async Task<IActionResult> RevokeMine(Guid sessionId, CancellationToken cancellationToken)
    {
        await refreshTokenService.RevokeSessionAsync(sessionId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("users/{userId:guid}/sessions")]
    [RequirePermission(Permissions.Identity.UsersManage)]
    public async Task<IActionResult> RevokeAllForUser(Guid userId, CancellationToken cancellationToken)
    {
        await refreshTokenService.RevokeAllForUserAsync(userId, cancellationToken);
        return NoContent();
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> LogoutByToken([FromBody] RevokeSessionRequest request, CancellationToken cancellationToken)
    {
        await refreshTokenService.RevokeAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

    private Guid ResolveUserId()
    {
        var raw = User.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
        if (!Guid.TryParse(raw, out var userId)) throw new InvalidOperationException("User context is missing.");
        return userId;
    }
}
