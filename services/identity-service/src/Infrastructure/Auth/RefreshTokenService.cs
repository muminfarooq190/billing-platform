using Microsoft.Extensions.Caching.Distributed;

namespace IdentityService.Infrastructure.Auth;

public sealed class RefreshTokenService(IDistributedCache distributedCache)
{
    public async Task<string> IssueRefreshTokenAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken)
    {
        var token = Guid.NewGuid().ToString("N");
        var key = $"refresh:{token}";
        var value = $"{userId}:{tenantId}";

        await distributedCache.SetStringAsync(
            key,
            value,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7) },
            cancellationToken);

        return token;
    }

    public async Task<(Guid UserId, Guid TenantId)?> ValidateAndRotateAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var key = $"refresh:{refreshToken}";
        var value = await distributedCache.GetStringAsync(key, cancellationToken);
        if (value is null)
        {
            return null;
        }

        await distributedCache.RemoveAsync(key, cancellationToken);
        var parts = value.Split(':', StringSplitOptions.RemoveEmptyEntries);
        return (Guid.Parse(parts[0]), Guid.Parse(parts[1]));
    }

    public Task RevokeAsync(string refreshToken, CancellationToken cancellationToken)
        => distributedCache.RemoveAsync($"refresh:{refreshToken}", cancellationToken);
}
