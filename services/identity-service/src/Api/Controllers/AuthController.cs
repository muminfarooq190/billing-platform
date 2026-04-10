using Dapper;
using IdentityService.Api.Contracts;
using IdentityService.Application.Commands.RegisterTenant;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.Enums;
using IdentityService.Domain.Repositories;
using IdentityService.Application.Queries.GetTenantByEmail;
using IdentityService.Infrastructure.Auth;
using IdentityService.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(IMediator mediator, JwtTokenService jwtTokenService, RefreshTokenService refreshTokenService, IConfiguration configuration, IUserRepository userRepository, IdentityDbContext dbContext) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RegisterTenantCommand(request.TenantName, request.Email, request.Password), cancellationToken);
        var token = jwtTokenService.GenerateAccessToken(result.OwnerUserId, result.TenantId, request.Email, "Owner");
        var refreshToken = await refreshTokenService.IssueRefreshTokenAsync(result.OwnerUserId, result.TenantId, cancellationToken);

        return Created(string.Empty, new { tenantId = result.TenantId, userId = result.OwnerUserId, accessToken = token, refreshToken });
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
                   "MustChangePassword" AS MustChangePassword
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

        var domainUser = await userRepository.GetByIdAsync(user.Id, cancellationToken);
        if (domainUser is not null)
        {
            domainUser.MarkLogin();
            await userRepository.UpdateAsync(domainUser, cancellationToken);
        }

        dbContext.SecurityEvents.Add(SecurityEvent.Create(user.TenantId, user.Id, "LoginSucceeded", HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), $"{{\"mustChangePassword\":{user.MustChangePassword.ToString().ToLowerInvariant()}}}"));
        await dbContext.SaveChangesAsync(cancellationToken);

        var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.TenantId, request.Email, user.Role);
        var refreshToken = await refreshTokenService.IssueRefreshTokenAsync(user.Id, user.TenantId, cancellationToken);
        return Ok(new { accessToken, refreshToken, mustChangePassword = user.MustChangePassword });
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
        var accessToken = jwtTokenService.GenerateAccessToken(userId, tenantId, "unknown@masked.local", "Member");
        var newRefreshToken = await refreshTokenService.IssueRefreshTokenAsync(userId, tenantId, cancellationToken);

        dbContext.SecurityEvents.Add(SecurityEvent.Create(tenantId, userId, "TokenRefreshed", HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), null));
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { accessToken, refreshToken = newRefreshToken });
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

    private sealed record UserAuthRow(Guid Id, Guid TenantId, string PasswordHash, string Role, string Status, bool MustChangePassword);
}
