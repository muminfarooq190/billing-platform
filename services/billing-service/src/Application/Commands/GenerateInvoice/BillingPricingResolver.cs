using System.Text.Json;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Enums;
using BillingService.Domain.Repositories;
using BillingService.Domain.ValueObjects;

namespace BillingService.Application.Commands.GenerateInvoice;

public interface IBillingPricingResolver
{
    Task<BillingPricingResult> ResolveAsync(Subscription subscription, CancellationToken cancellationToken);
}

public sealed record BillingPricingResult(
    IReadOnlyList<InvoiceLineItem> LineItems,
    Money TaxAmount,
    DateOnly BillingPeriodStart,
    DateOnly BillingPeriodEnd,
    string PricingReference);

public sealed class BillingPricingResolver(
    ITenantSubscriptionPackageRepository tenantSubscriptionPackageRepository,
    ICommercialPackageRepository commercialPackageRepository) : IBillingPricingResolver
{
    private const string DefaultCurrency = "USD";

    public async Task<BillingPricingResult> ResolveAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        var assignments = await tenantSubscriptionPackageRepository.ListByTenantIdAsync(subscription.TenantId, cancellationToken);
        var effective = assignments
            .Where(x => x.IsEffectiveAt(subscription.NextBillingDate))
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefault();

        var lineItems = new List<InvoiceLineItem>();
        string pricingReference;

        if (effective is not null)
        {
            var package = await commercialPackageRepository.GetByIdAsync(effective.CommercialPackageId, cancellationToken);
            if (package is not null)
            {
                var price = ResolvePackagePrice(package.MetadataJson, subscription.BillingCycle, subscription.PlanType);
                lineItems.Add(new InvoiceLineItem($"{package.Name} ({subscription.BillingCycle})", 1, price));
                pricingReference = $"package:{package.Code}";
            }
            else
            {
                var fallback = ResolvePlanFallbackPrice(subscription.PlanType, subscription.BillingCycle);
                lineItems.Add(new InvoiceLineItem($"Unmapped package fallback ({subscription.BillingCycle})", 1, fallback));
                pricingReference = "fallback:missing-package";
            }
        }
        else
        {
            var fallback = ResolvePlanFallbackPrice(subscription.PlanType, subscription.BillingCycle);
            lineItems.Add(new InvoiceLineItem($"Package pricing fallback ({subscription.BillingCycle})", 1, fallback));
            pricingReference = "fallback:no-package-assignment";
        }

        var subtotal = lineItems.Select(x => x.LineTotal).Aggregate(new Money(0m, lineItems[0].UnitPrice.Currency), (acc, next) => acc.Add(next));
        var tax = new Money(decimal.Round(subtotal.Amount * 0.10m, 4), subtotal.Currency);
        var periodEnd = subscription.BillingCycle == BillingCycle.Monthly
            ? DateOnly.FromDateTime(subscription.NextBillingDate.UtcDateTime.Date.AddMonths(1).AddDays(-1))
            : DateOnly.FromDateTime(subscription.NextBillingDate.UtcDateTime.Date.AddYears(1).AddDays(-1));

        return new BillingPricingResult(
            lineItems,
            tax,
            DateOnly.FromDateTime(subscription.NextBillingDate.UtcDateTime.Date),
            periodEnd,
            pricingReference);
    }

    private static Money ResolvePackagePrice(string? metadataJson, BillingCycle billingCycle, PlanType planType)
    {
        if (!string.IsNullOrWhiteSpace(metadataJson))
        {
            try
            {
                using var document = JsonDocument.Parse(metadataJson);
                if (document.RootElement.TryGetProperty("pricing", out var pricing) && pricing.ValueKind == JsonValueKind.Object)
                {
                    var cycleKey = billingCycle == BillingCycle.Monthly ? "monthly" : "annual";
                    if (pricing.TryGetProperty(cycleKey, out var amountElement) && amountElement.TryGetDecimal(out var amount))
                    {
                        return new Money(decimal.Round(amount, 4), DefaultCurrency);
                    }
                }
            }
            catch (JsonException)
            {
                // Fall back below; bad metadata should not take billing down.
            }
        }

        return ResolvePlanFallbackPrice(planType, billingCycle);
    }

    private static Money ResolvePlanFallbackPrice(PlanType planType, BillingCycle billingCycle)
    {
        var amount = (planType, billingCycle) switch
        {
            (PlanType.Free, BillingCycle.Monthly) => 0m,
            (PlanType.Free, BillingCycle.Annual) => 0m,
            (PlanType.Pro, BillingCycle.Monthly) => 49m,
            (PlanType.Pro, BillingCycle.Annual) => 490m,
            (PlanType.Enterprise, BillingCycle.Monthly) => 199m,
            (PlanType.Enterprise, BillingCycle.Annual) => 1990m,
            _ => 49m
        };

        return new Money(decimal.Round(amount, 4), DefaultCurrency);
    }
}
