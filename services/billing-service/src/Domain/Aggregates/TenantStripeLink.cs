namespace BillingService.Domain.Aggregates;

/// <summary>
/// Link between a tenant and the Stripe `Customer` object that holds their
/// saved payment methods. One row per tenant.
///
/// Stripe Customer = persistent identity across charges. Without this link
/// every Checkout Session creates a fresh anonymous Customer, so:
///   - card-on-file does not work across invoices
///   - saved payment methods cannot be reused for off-session charges
///   - per-customer Stripe Dashboard view shows a noisy splatter of orphans
///
/// Kept as a thin entity (no domain behavior) — it's an integration
/// projection, not part of any aggregate's invariant set.
/// </summary>
public sealed class TenantStripeLink
{
    private TenantStripeLink() { }

    private TenantStripeLink(Guid tenantId, string stripeCustomerId)
    {
        TenantId = tenantId;
        StripeCustomerId = stripeCustomerId;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid TenantId { get; private set; }
    public string StripeCustomerId { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static TenantStripeLink Create(Guid tenantId, string stripeCustomerId)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(stripeCustomerId)) throw new ArgumentException("Stripe customer id is required.", nameof(stripeCustomerId));
        return new TenantStripeLink(tenantId, stripeCustomerId.Trim());
    }

    /// <summary>Replace the Stripe customer id (e.g. after a manual reissue).</summary>
    public void UpdateCustomerId(string stripeCustomerId)
    {
        if (string.IsNullOrWhiteSpace(stripeCustomerId)) throw new ArgumentException("Stripe customer id is required.", nameof(stripeCustomerId));
        StripeCustomerId = stripeCustomerId.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
