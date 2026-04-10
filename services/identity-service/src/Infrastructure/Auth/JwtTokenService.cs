using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Infrastructure.Auth;

public sealed class JwtTokenService(IConfiguration configuration)
{
    public string GenerateAccessToken(Guid userId, Guid tenantId, string email, string role, IReadOnlyCollection<string>? permissions = null, bool mfaVerified = false)
    {
        var privateKeyPem = LoadPem("JWT_PRIVATE_KEY", "JWT_PRIVATE_KEY_PATH") ?? throw new InvalidOperationException("JWT private key is not configured.");
        var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);

        var key = new RsaSecurityKey(rsa) { KeyId = "identity-key-1" };
        var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
        var issuer = configuration["JWT_ISSUER"] ?? "billing-platform.identity";
        var audience = configuration["JWT_AUDIENCE"] ?? "billing-platform.clients";

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new("tenant_id", tenantId.ToString()),
            new("tenantId", tenantId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Role, role),
            new("mfa_verified", mfaVerified.ToString().ToLowerInvariant()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (permissions is not null)
        {
            claims.AddRange(permissions.Distinct(StringComparer.OrdinalIgnoreCase).Select(x => new Claim("permission", x)));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string BuildJwks()
    {
        var publicKeyPem = LoadPem("JWT_PUBLIC_KEY", "JWT_PUBLIC_KEY_PATH") ?? throw new InvalidOperationException("JWT public key is not configured.");
        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        var parameters = rsa.ExportParameters(false);

        return System.Text.Json.JsonSerializer.Serialize(new
        {
            keys = new[]
            {
                new
                {
                    kty = "RSA",
                    use = "sig",
                    kid = "identity-key-1",
                    alg = "RS256",
                    n = Base64UrlEncoder.Encode(parameters.Modulus),
                    e = Base64UrlEncoder.Encode(parameters.Exponent)
                }
            }
        });
    }

    private string? LoadPem(string inlineKeyName, string pathKeyName)
    {
        var inlinePem = configuration[inlineKeyName];
        if (!string.IsNullOrWhiteSpace(inlinePem))
        {
            return inlinePem;
        }

        var path = configuration[pathKeyName];
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            return File.ReadAllText(path);
        }

        return null;
    }
}
