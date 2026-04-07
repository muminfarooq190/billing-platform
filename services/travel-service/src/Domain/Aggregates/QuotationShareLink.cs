using TravelService.Domain.Exceptions;

namespace TravelService.Domain.Aggregates;

public sealed class QuotationShareLink
{
    private QuotationShareLink() { }

    private QuotationShareLink(Guid quotationId, Guid quotationRevisionId, Guid tenantId, string token, DateTimeOffset? expiresAt)
    {
        if (quotationId == Guid.Empty)
            throw new DomainException("QuotationId is required.");
        if (quotationRevisionId == Guid.Empty)
            throw new DomainException("QuotationRevisionId is required.");
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(token))
            throw new DomainException("Share token is required.");

        Id = Guid.NewGuid();
        QuotationId = quotationId;
        QuotationRevisionId = quotationRevisionId;
        TenantId = tenantId;
        Token = token.Trim();
        ExpiresAt = expiresAt;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid QuotationId { get; private set; }
    public Guid QuotationRevisionId { get; private set; }
    public Guid TenantId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTimeOffset? ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public DateTimeOffset? LastViewedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static QuotationShareLink Create(Guid quotationId, Guid quotationRevisionId, Guid tenantId, string token, DateTimeOffset? expiresAt)
        => new(quotationId, quotationRevisionId, tenantId, token, expiresAt);

    public bool IsActive(DateTimeOffset now)
        => RevokedAt is null && (!ExpiresAt.HasValue || ExpiresAt.Value >= now);

    public void MarkViewed(DateTimeOffset? viewedAt = null)
    {
        LastViewedAt = viewedAt ?? DateTimeOffset.UtcNow;
    }

    public void Revoke(DateTimeOffset? revokedAt = null)
    {
        RevokedAt = revokedAt ?? DateTimeOffset.UtcNow;
    }
}
