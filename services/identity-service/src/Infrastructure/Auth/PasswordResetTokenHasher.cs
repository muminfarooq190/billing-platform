using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Infrastructure.Auth;

public static class PasswordResetTokenHasher
{
    public static string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
