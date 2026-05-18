using System.Security.Cryptography;
using System.Text;
using IdentityService.Api.Contracts;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.Enums;
using IdentityService.Domain.Exceptions;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Controllers;

/// <summary>
/// Anonymous-accessible invitation lookup + acceptance.
///
/// Frontend (voyara-portal) hits <c>/identity/invitations/{token}</c> and
/// <c>/identity/invitations/accept</c>. The matching gateway bypass is
/// <c>/api/identity/invitations/</c> (see <c>JwtValidationMiddleware</c>).
///
/// The legacy authenticated routes under <c>/identity/users/invitations</c>
/// remain for admins to create/resend.
/// </summary>
[ApiController]
[Route("identity/invitations")]
[AllowAnonymous]
public sealed class InvitationsController(IdentityDbContext dbContext, IUserRepository userRepository) : ControllerBase
{
    [HttpGet("{token}")]
    public async Task<IActionResult> Preview(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new NotFoundException("Invitation not found.");
        }

        var tokenHash = Hash(token);
        var invitation = await dbContext.UserInvitations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken)
            ?? throw new NotFoundException("Invitation not found.");

        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == invitation.TenantId, cancellationToken);
        var invitedBy = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == invitation.InvitedByUserId, cancellationToken);

        var status = invitation.AcceptedAt.HasValue
            ? "Accepted"
            : invitation.RevokedAt.HasValue
                ? "Revoked"
                : invitation.ExpiresAt <= DateTimeOffset.UtcNow
                    ? "Expired"
                    : "Pending";

        return Ok(new
        {
            id = invitation.Id,
            email = invitation.Email,
            role = invitation.Role,
            expiresAt = invitation.ExpiresAt,
            acceptedAt = invitation.AcceptedAt,
            status,
            tenantName = tenant?.Name,
            invitedByEmail = invitedBy?.Email,
        });
    }

    [HttpPost("accept")]
    public async Task<IActionResult> Accept([FromBody] AcceptUserInvitationRequest request, CancellationToken cancellationToken)
    {
        var tokenHash = Hash(request.Token);
        var invitation = await dbContext.UserInvitations
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken)
            ?? throw new NotFoundException("Invitation not found.");

        invitation.Accept();

        var user = await userRepository.GetByTenantAndEmailAsync(invitation.TenantId, invitation.Email, cancellationToken)
            ?? throw new NotFoundException("Invited user not found.");

        user.AcceptInvitation(BCrypt.Net.BCrypt.HashPassword(request.Password));
        await userRepository.UpdateAsync(user, cancellationToken);

        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == invitation.TenantId, cancellationToken);

        dbContext.SecurityEvents.Add(SecurityEvent.Create(
            invitation.TenantId,
            user.Id,
            "InvitationAccepted",
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            $"{{\"invitationId\":\"{invitation.Id}\"}}"));
        dbContext.IdentityAuditLogs.Add(IdentityAuditLog.Create(
            invitation.TenantId,
            user.Id,
            user.Id,
            "UserInvitationAccepted",
            null,
            $"{{\"status\":\"{user.Status}\"}}",
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString()));

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            userId = user.Id,
            email = user.Email,
            tenantId = invitation.TenantId,
            tenantName = tenant?.Name,
            role = invitation.Role,
        });
    }

    private static string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
