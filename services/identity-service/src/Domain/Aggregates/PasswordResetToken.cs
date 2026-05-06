using IdentityService.Domain.Common;
using IdentityService.Domain.Exceptions;

namespace IdentityService.Domain.Aggregates;

public sealed class PasswordResetToken : AggregateRoot
{
    private PasswordResetToken() { }

    private PasswordResetToken(Guid id, Guid userId, string email, string tokenHash, DateTimeOffset expiresAt)
    {
        Id = id;
        UserId = userId;
        Email = email;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? ConsumedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static PasswordResetToken Create(Guid userId, string email, string tokenHash, DateTimeOffset expiresAt)
    {
        if (userId == Guid.Empty) throw new DomainException("User id is required.");
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Email is required.");
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new DomainException("Token hash is required.");
        if (expiresAt <= DateTimeOffset.UtcNow) throw new DomainException("Password reset expiry must be in the future.");

        return new PasswordResetToken(Guid.NewGuid(), userId, email.Trim().ToLowerInvariant(), tokenHash, expiresAt);
    }

    public void Consume()
    {
        if (ConsumedAt.HasValue) throw new DomainException("Password reset token has already been consumed.");
        if (ExpiresAt <= DateTimeOffset.UtcNow) throw new DomainException("Password reset token has expired.");

        ConsumedAt = DateTimeOffset.UtcNow;
    }
}
