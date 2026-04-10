using IdentityService.Domain.Aggregates;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace IdentityService.Infrastructure.Auth;

public sealed class RefreshTokenService(IDistributedCache distributedCache, IdentityDbContext dbContext, IHttpContextAccessor httpContextAccessor)
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

        var http = httpContextAccessor.HttpContext;
        var session = UserSession.Create(
            tenantId,
            userId,
            token,
            http?.Request.Headers["X-Device-Name"].FirstOrDefault(),
            http?.Connection.RemoteIpAddress?.ToString(),
            http?.Request.Headers.UserAgent.ToString());

        dbContext.UserSessions.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);
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

        var session = await dbContext.UserSessions.FirstOrDefaultAsync(x => x.RefreshTokenId == refreshToken && x.RevokedAt == null, cancellationToken);
        if (session is null)
        {
            await distributedCache.RemoveAsync(key, cancellationToken);
            return null;
        }

        session.Touch(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(), httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString());
        session.Revoke();
        await dbContext.SaveChangesAsync(cancellationToken);
        await distributedCache.RemoveAsync(key, cancellationToken);
        var parts = value.Split(':', StringSplitOptions.RemoveEmptyEntries);
        return (Guid.Parse(parts[0]), Guid.Parse(parts[1]));
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var session = await dbContext.UserSessions.FirstOrDefaultAsync(x => x.RefreshTokenId == refreshToken, cancellationToken);
        if (session is not null)
        {
            session.Revoke();
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await distributedCache.RemoveAsync($"refresh:{refreshToken}", cancellationToken);
    }

    public async Task<IReadOnlyList<UserSession>> ListSessionsAsync(Guid userId, CancellationToken cancellationToken)
        => await dbContext.UserSessions.AsNoTracking().Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

    public async Task RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await dbContext.UserSessions.FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);
        if (session is null) return;
        session.Revoke();
        await dbContext.SaveChangesAsync(cancellationToken);
        await distributedCache.RemoveAsync($"refresh:{session.RefreshTokenId}", cancellationToken);
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var sessions = await dbContext.UserSessions.Where(x => x.UserId == userId && x.RevokedAt == null).ToListAsync(cancellationToken);
        foreach (var session in sessions)
        {
            session.Revoke();
            await distributedCache.RemoveAsync($"refresh:{session.RefreshTokenId}", cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
