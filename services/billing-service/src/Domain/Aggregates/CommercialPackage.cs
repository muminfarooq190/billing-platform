using BillingService.Domain.Common;
using BillingService.Domain.Exceptions;

namespace BillingService.Domain.Aggregates;

public sealed class CommercialPackage : AggregateRoot
{
    private CommercialPackage() { }

    private CommercialPackage(string code, string name, string category, string billingModel, string description, bool isActive, string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Code is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");
        if (string.IsNullOrWhiteSpace(category))
            throw new DomainException("Category is required.");
        if (string.IsNullOrWhiteSpace(billingModel))
            throw new DomainException("Billing model is required.");
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required.");

        Id = Guid.NewGuid();
        Code = code.Trim();
        Name = name.Trim();
        Category = category.Trim();
        BillingModel = billingModel.Trim();
        Description = description.Trim();
        IsActive = isActive;
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string BillingModel { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public string? MetadataJson { get; private set; }
    /// <summary>
    /// Stripe Price id (`price_*`) for the monthly billing cycle. When set,
    /// subscriptions for this package go through Stripe-native recurring
    /// billing instead of our cron-driven invoice generator. Null = falls
    /// back to legacy cron path.
    /// </summary>
    public string? StripePriceIdMonthly { get; private set; }
    /// <summary>Stripe Price id for the annual billing cycle. Same semantics.</summary>
    public string? StripePriceIdAnnual { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static CommercialPackage Create(string code, string name, string category, string billingModel, string description, bool isActive = true, string? metadataJson = null)
        => new(code, name, category, billingModel, description, isActive, metadataJson);

    public void Update(string code, string name, string category, string billingModel, string description, bool isActive, string? metadataJson)
    {
        if (DeletedAt is not null)
            throw new DomainException("Cannot update a deleted commercial package.");

        Code = code.Trim();
        Name = name.Trim();
        Category = category.Trim();
        BillingModel = billingModel.Trim();
        Description = description.Trim();
        IsActive = isActive;
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Bind Stripe Price ids (created in the Stripe dashboard or via the
    /// Stripe API) to this package. Both nullable so partial wiring is
    /// allowed (e.g. monthly-only package).
    /// </summary>
    public void SetStripePrices(string? monthlyPriceId, string? annualPriceId)
    {
        if (DeletedAt is not null)
            throw new DomainException("Cannot update a deleted commercial package.");

        StripePriceIdMonthly = string.IsNullOrWhiteSpace(monthlyPriceId) ? null : monthlyPriceId.Trim();
        StripePriceIdAnnual = string.IsNullOrWhiteSpace(annualPriceId) ? null : annualPriceId.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public string? GetStripePriceFor(Enums.BillingCycle cycle) => cycle == Enums.BillingCycle.Monthly
        ? StripePriceIdMonthly
        : StripePriceIdAnnual;
}
