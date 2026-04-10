using System.Security.Cryptography;
using System.Text;
using IdentityService.Api.Contracts;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.Enums;
using IdentityService.Domain.Exceptions;
using IdentityService.Domain.Repositories;
using IdentityService.Domain.ValueObjects;
using IdentityService.Infrastructure.Auth;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("identity/users/invitations")]
[RequirePermission(Permissions.Identity.UsersManage)]
public sealed class UserInvitationsController(IdentityDbContext dbContext, IUserRepository userRepository) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserInvitationRequest request, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        var actorUserId = ResolveActorUserId();
        var existingUser = await userRepository.GetByTenantAndEmailAsync(tenantId, request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (existingUser is not null)
        {
            throw new ConflictException("User already exists for this tenant.");
        }

        var role = Enum.Parse<UserRole>(request.Role, true);
        var rawToken = Guid.NewGuid().ToString("N");
        var invitation = UserInvitation.Create(tenantId, request.Email, role.ToString(), request.InvitedByUserId ?? actorUserId, Hash(rawToken), DateTimeOffset.UtcNow.AddHours(Math.Max(1, request.ExpiresInHours)));
        dbContext.UserInvitations.Add(invitation);

        var invitedUser = IdentityService.Domain.Aggregates.User.Invite(new TenantId(tenantId), new Email(request.Email), role);
        await userRepository.AddAsync(invitedUser, cancellationToken);

        dbContext.IdentityAuditLogs.Add(IdentityAuditLog.Create(tenantId, actorUserId, invitedUser.Id, "UserInvitationCreated", null, $"{{\"email\":\"{invitation.Email}\",\"role\":\"{invitation.Role}\"}}", HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString()));
        dbContext.SecurityEvents.Add(SecurityEvent.Create(tenantId, invitedUser.Id, "InvitationCreated", HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), $"{{\"invitationId\":\"{invitation.Id}\"}}"));

        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Create), new { id = invitation.Id }, new { invitation.Id, invitation.Email, invitation.Role, invitation.ExpiresAt, token = rawToken, invitedUserId = invitedUser.Id });
    }

    [HttpPost("{id:guid}/resend")]
    public async Task<IActionResult> Resend(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        var invitation = await dbContext.UserInvitations.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken)
            ?? throw new NotFoundException("Invitation not found.");

        var rawToken = Guid.NewGuid().ToString("N");
        invitation.Resend(Hash(rawToken), DateTimeOffset.UtcNow.AddHours(72));
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { invitation.Id, invitation.Email, invitation.ExpiresAt, token = rawToken });
    }

    [HttpPost("accept")]
    [AllowAnonymous]
    public async Task<IActionResult> Accept([FromBody] AcceptUserInvitationRequest request, CancellationToken cancellationToken)
    {
        var tokenHash = Hash(request.Token);
        var invitation = await dbContext.UserInvitations.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken)
            ?? throw new NotFoundException("Invitation not found.");

        invitation.Accept();

        var user = await userRepository.GetByTenantAndEmailAsync(invitation.TenantId, invitation.Email, cancellationToken)
            ?? throw new NotFoundException("Invited user not found.");

        user.AcceptInvitation(BCrypt.Net.BCrypt.HashPassword(request.Password));
        await userRepository.UpdateAsync(user, cancellationToken);

        dbContext.SecurityEvents.Add(SecurityEvent.Create(invitation.TenantId, user.Id, "InvitationAccepted", HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), $"{{\"invitationId\":\"{invitation.Id}\"}}"));
        dbContext.IdentityAuditLogs.Add(IdentityAuditLog.Create(invitation.TenantId, user.Id, user.Id, "UserInvitationAccepted", null, $"{{\"status\":\"{user.Status}\"}}", HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString()));

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { user.Id, user.Email, Status = user.Status.ToString() });
    }

    private Guid ResolveTenantId()
    {
        var raw = User.Claims.FirstOrDefault(x => x.Type == "tenantId")?.Value ?? Request.Headers["X-Tenant-Id"].FirstOrDefault();
        if (!Guid.TryParse(raw, out var tenantId)) throw new InvalidOperationException("Tenant context is missing.");
        return tenantId;
    }

    private Guid ResolveActorUserId()
    {
        var raw = User.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
        return Guid.TryParse(raw, out var userId) ? userId : Guid.Empty;
    }

    private static string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
