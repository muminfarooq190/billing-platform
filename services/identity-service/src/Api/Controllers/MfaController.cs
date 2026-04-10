using System.Security.Cryptography;
using System.Text.Json;
using IdentityService.Api.Contracts;
using IdentityService.Domain.Aggregates;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtpNet;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("identity/me/mfa")]
[Authorize]
public sealed class MfaController(IdentityDbContext dbContext) : ControllerBase
{
    [HttpPost("enroll")]
    public async Task<IActionResult> Enroll([FromBody] MfaEnrollRequest request, CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        var tenantId = ResolveTenantId();
        var existing = await dbContext.UserMfaEnrollments.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, cancellationToken);
        if (existing is not null && existing.IsEnabled)
        {
            return Ok(new { alreadyEnabled = true });
        }

        var secretBytes = RandomNumberGenerator.GetBytes(20);
        var secret = Base32Encoding.ToString(secretBytes);
        var recoveryCodes = Enumerable.Range(0, 8).Select(_ => Convert.ToHexString(RandomNumberGenerator.GetBytes(4))).ToArray();
        var recoveryJson = JsonSerializer.Serialize(recoveryCodes);

        if (existing is null)
        {
            existing = UserMfaEnrollment.Create(tenantId, userId, secret, recoveryJson);
            dbContext.UserMfaEnrollments.Add(existing);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        var otpUrl = new OtpUri(OtpType.Totp, secretBytes, User.Identity?.Name ?? userId.ToString(), "Voyara").ToString();
        return Ok(new { secret, recoveryCodes, otpUrl, deviceName = request.DeviceName });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] MfaVerifyRequest request, CancellationToken cancellationToken)
    {
        var enrollment = await RequireEnrollment(cancellationToken);
        var totp = new Totp(Base32Encoding.ToBytes(enrollment.Secret));
        var valid = totp.VerifyTotp(request.Code.Replace(" ", string.Empty), out _, new VerificationWindow(1, 1));
        if (!valid)
        {
            return Unauthorized(new { code = "mfa_invalid", message = "Invalid MFA code." });
        }

        enrollment.Verify();
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { enabled = true, verifiedAt = enrollment.VerifiedAt });
    }

    [HttpPost("disable")]
    public async Task<IActionResult> Disable([FromBody] MfaDisableRequest request, CancellationToken cancellationToken)
    {
        var enrollment = await RequireEnrollment(cancellationToken);
        var totp = new Totp(Base32Encoding.ToBytes(enrollment.Secret));
        var valid = totp.VerifyTotp(request.Code.Replace(" ", string.Empty), out _, new VerificationWindow(1, 1));
        if (!valid)
        {
            return Unauthorized(new { code = "mfa_invalid", message = "Invalid MFA code." });
        }

        enrollment.Disable();
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { enabled = false });
    }

    private async Task<UserMfaEnrollment> RequireEnrollment(CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        var tenantId = ResolveTenantId();
        return await dbContext.UserMfaEnrollments.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("MFA enrollment not found.");
    }

    private Guid ResolveTenantId()
    {
        var raw = User.Claims.FirstOrDefault(x => x.Type == "tenantId" || x.Type == "tenant_id")?.Value ?? Request.Headers["X-Tenant-Id"].FirstOrDefault();
        if (!Guid.TryParse(raw, out var tenantId)) throw new InvalidOperationException("Tenant context is missing.");
        return tenantId;
    }

    private Guid ResolveUserId()
    {
        var raw = User.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
        if (!Guid.TryParse(raw, out var userId)) throw new InvalidOperationException("User context is missing.");
        return userId;
    }
}
