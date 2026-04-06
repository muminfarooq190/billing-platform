using Dapper;
using IdentityService.Api.Contracts;
using IdentityService.Application.Commands.RegisterTenant;
using IdentityService.Domain.Repositories;
using IdentityService.Application.Queries.GetTenantByEmail;
using IdentityService.Infrastructure.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(IMediator mediator, JwtTokenService jwtTokenService, RefreshTokenService refreshTokenService, IConfiguration configuration, IUserRepository userRepository) : ControllerBase
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
                   "Role" AS Role
            FROM users
            WHERE "TenantId" = @TenantId AND "Email" = @Email AND deleted_at IS NULL;
            """;

        var user = await connection.QuerySingleOrDefaultAsync<UserAuthRow>(new CommandDefinition(sql, new { TenantId = tenant.Id, Email = request.Email.ToLowerInvariant() }, cancellationToken: cancellationToken));
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        var domainUser = await userRepository.GetByIdAsync(user.Id, cancellationToken);
        if (domainUser is not null)
        {
            domainUser.MarkLogin();
            await userRepository.UpdateAsync(domainUser, cancellationToken);
        }

        var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.TenantId, request.Email, user.Role);
        var refreshToken = await refreshTokenService.IssueRefreshTokenAsync(user.Id, user.TenantId, cancellationToken);
        return Ok(new { accessToken, refreshToken });
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

    private sealed record UserAuthRow(Guid Id, Guid TenantId, string PasswordHash, string Role);
}
