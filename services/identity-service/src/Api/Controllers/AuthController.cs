using Dapper;
using IdentityService.Api.Contracts;
using IdentityService.Application.Commands.RegisterTenant;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.Enums;
using IdentityService.Domain.Repositories;
using IdentityService.Application.Queries.GetTenantByEmail;
using IdentityService.Domain.Exceptions;
using IdentityService.Infrastructure.Auth;
using IdentityService.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OtpNet;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(IMediator mediator, JwtTokenService jwtTokenService, RefreshTokenService refreshTokenService, IConfiguration configuration, IUserRepository userRepository, IdentityDbContext dbContext) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RegisterTenantCommand(request.TenantName, request.Email, request.Password), cancellationToken);

        var ownerRole = await dbContext.RoleDefinitions.AsNoTracking().FirstAsync(x => x.TenantId == null && x.NormalizedName == "OWNER", cancellationToken);
        dbContext.UserRoleAssignments.Add(UserRoleAssignment.Create(result.TenantId, result.OwnerUserId, ownerRole.Id));
        await dbContext.SaveChangesAsync(cancellationToken);

        var permissions = await ResolvePermissionsAsync(result.OwnerUserId, result.TenantId, cancellationToken);
        var token = jwtTokenService.GenerateAccessToken(result.OwnerUserId, result.TenantId, request.Email, "Owner", permissions, false);
        var refreshToken = await refreshTokenService.IssueRefreshTokenAsync(result.OwnerUserId, result.TenantId, cancellationToken);

        return Created(string.Empty, new { tenantId = result.TenantId, userId = result.OwnerUserId, accessToken = token, refreshToken, permissions });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var tenant = await mediator.Send(new GetTenantByEmailQuery(request.Email), cancellationToken);
        if (tenant is null)
        {
            return Unauthorized();
        }

        await using var connection = new NpgsqlConnection(configuration["DATABASE_URL"]);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT "Id" AS Id,
                   "TenantId" AS TenantId,
                   "PasswordHash" AS PasswordHash,
                   "Role" AS Role,
                   "Status" AS Status,
                   must_change_password AS MustChangePassword
            FROM users
            WHERE "TenantId" = @TenantId AND "Email" = @Email AND deleted_at IS NULL;
            """;

        var user = await connection.QuerySingleOrDefaultAsync<UserAuthRow>(new CommandDefinition(sql, new { TenantId = tenant.Id, Email = request.Email.ToLowerInvariant() }, cancellationToken: cancellationToken));
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            dbContext.SecurityEvents.Add(SecurityEvent.Create(tenant.Id, null, "LoginFailed", HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), $"{{\"email\":\"{request.Email.ToLowerInvariant()}\"}}"));
            await dbContext.SaveChangesAsync(cancellationToken);
            return Unauthorized();
        }

        if (!string.Equals(user.Status, UserStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            dbContext.SecurityEvents.Add(SecurityEvent.Create(user.TenantId, user.Id, "LoginDenied", HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), $"{{\"status\":\"{user.Status}\"}}"));
            await dbContext.SaveChangesAsync(cancellationToken);
            return StatusCode(StatusCodes.Status403Forbidden, new { code = "user_inactive", message = $"User is {user.Status}." });
        }

        var mfa = await dbContext.UserMfaEnrollments.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == user.TenantId && x.UserId == user.Id && x.VerifiedAt != null && x.DisabledAt == null, cancellationToken);
        var mfaVerified = false;
        if (mfa is not null)
        {
            if (string.IsNullOrWhiteSpace(request.MfaCode))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { code = "mfa_required", message = "MFA code is required." });
            }

            var totp = new Totp(Base32Encoding.ToBytes(mfa.Secret));
            mfaVerified = totp.VerifyTotp(request.MfaCode.Replace(" ", string.Empty), out _, new VerificationWindow(1, 1));
            if (!mfaVerified)
            {
                dbContext.SecurityEvents.Add(SecurityEvent.Create(user.TenantId, user.Id, "LoginDeniedMfa", HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), null));
                await dbContext.SaveChangesAsync(cancellationToken);
                return StatusCode(StatusCodes.Status403Forbidden, new { code = "mfa_invalid", message = "Invalid MFA code." });
            }
        }

        var domainUser = await userRepository.GetByIdAsync(user.Id, cancellationToken);
        if (domainUser is not null)
        {
            domainUser.MarkLogin();
            await userRepository.UpdateAsync(domainUser, cancellationToken);
        }

        var permissions = await ResolvePermissionsAsync(user.Id, user.TenantId, cancellationToken);
        dbContext.SecurityEvents.Add(SecurityEvent.Create(user.TenantId, user.Id, "LoginSucceeded", HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), $"{{\"mustChangePassword\":{user.MustChangePassword.ToString().ToLowerInvariant()},\"mfaVerified\":{mfaVerified.ToString().ToLowerInvariant()}}}"));
        await dbContext.SaveChangesAsync(cancellationToken);

        var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.TenantId, request.Email, user.Role, permissions, mfaVerified);
        var refreshToken = await refreshTokenService.IssueRefreshTokenAsync(user.Id, user.TenantId, cancellationToken);
        return Ok(new { accessToken, refreshToken, mustChangePassword = user.MustChangePassword, permissions, mfaVerified });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var tenant = await mediator.Send(new GetTenantByEmailQuery(email), cancellationToken);
        if (tenant is null)
        {
            return Ok(new { accepted = true });
        }

        var user = await userRepository.GetByTenantAndEmailAsync(tenant.Id, email, cancellationToken);
        if (user is null || user.Status != UserStatus.Active)
        {
            return Ok(new { accepted = true });
        }

        var existingTokens = await dbContext.PasswordResetTokens
            .Where(x => x.UserId == user.Id && x.ConsumedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var token in existingTokens)
        {
            if (token.ExpiresAt > DateTimeOffset.UtcNow)
            {
                token.Consume();
            }
        }

        var resetToken = user.RequestPasswordReset(TimeSpan.FromHours(1));
        dbContext.PasswordResetTokens.Add(resetToken);
        await userRepository.UpdateAsync(user, cancellationToken);
        return Ok(new { accepted = true });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var tokenHash = PasswordResetTokenHasher.Hash(request.Token);
        var resetToken = await dbContext.PasswordResetTokens
            .FirstOrDefaultAsync(x => x.Email == email && x.TokenHash == tokenHash, cancellationToken)
            ?? throw new NotFoundException("Password reset token not found.");

        resetToken.Consume();

        var tenant = await mediator.Send(new GetTenantByEmailQuery(email), cancellationToken)
            ?? throw new NotFoundException("Tenant not found for email.");
        var user = await userRepository.GetByTenantAndEmailAsync(tenant.Id, email, cancellationToken)
            ?? throw new NotFoundException("User not found for password reset.");

        user.ChangePassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
        await userRepository.UpdateAsync(user, cancellationToken);
        await refreshTokenService.RevokeAllForUserAsync(user.Id, cancellationToken);

        dbContext.SecurityEvents.Add(SecurityEvent.Create(user.TenantId, user.Id, "PasswordResetSucceeded", HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), null));
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var pair = await refreshTokenService.ValidateAndRotateAsync(request.RefreshToken, cancellationToken);
        if (pair is null)
        {
            return Unauthorized();
        }

        var (userId, tenantId) = pair.Value;
        var permissions = await ResolvePermissionsAsync(userId, tenantId, cancellationToken);
        var accessToken = jwtTokenService.GenerateAccessToken(userId, tenantId, "unknown@masked.local", "Member", permissions, false);
        var newRefreshToken = await refreshTokenService.IssueRefreshTokenAsync(userId, tenantId, cancellationToken);

        dbContext.SecurityEvents.Add(SecurityEvent.Create(tenantId, userId, "TokenRefreshed", HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), null));
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { accessToken, refreshToken = newRefreshToken, permissions });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        await refreshTokenService.RevokeAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

    [HttpGet("/.well-known/jwks.json")]
    public ContentResult GetJwks()
    {
        return Content(jwtTokenService.BuildJwks(), "application/json");
    }

    private async Task<IReadOnlyList<string>> ResolvePermissionsAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken)
    {
        return await (from assignment in dbContext.UserRoleAssignments.AsNoTracking()
                      join role in dbContext.RoleDefinitions.AsNoTracking() on assignment.RoleDefinitionId equals role.Id
                      join permission in dbContext.RolePermissionAssignments.AsNoTracking() on role.Id equals permission.RoleDefinitionId
                      where assignment.UserId == userId && assignment.TenantId == tenantId
                      select permission.PermissionKey)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
    }

    private sealed record UserAuthRow(Guid Id, Guid TenantId, string PasswordHash, string Role, string Status, bool MustChangePassword);
}
