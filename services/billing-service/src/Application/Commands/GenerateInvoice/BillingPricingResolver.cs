using System.Text.Json;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Enums;
using BillingService.Domain.Exceptions;
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

        if (effective is null)
            throw new DomainException("No active commercial package assignment was found for the tenant at the billing date.");

        var package = await commercialPackageRepository.GetByIdAsync(effective.CommercialPackageId, cancellationToken)
            ?? throw new DomainException($"Commercial package '{effective.CommercialPackageId}' was not found for billing.");

        var packagePrice = ResolvePackagePrice(package.MetadataJson, subscription.BillingCycle);
        var lineItems = new List<InvoiceLineItem>
        {
            new($"{package.Name} ({subscription.BillingCycle})", 1, packagePrice)
        };

        var subtotal = lineItems.Select(x => x.LineTotal).Aggregate(new Money(0m, lineItems[0].UnitPrice.Currency), (acc, next) => acc.Add(next));
        var taxRate = ResolveTaxRate(package.MetadataJson);
        var tax = new Money(decimal.Round(subtotal.Amount * taxRate, 4), subtotal.Currency);
        var periodEnd = subscription.BillingCycle == BillingCycle.Monthly
            ? DateOnly.FromDateTime(subscription.NextBillingDate.UtcDateTime.Date.AddMonths(1).AddDays(-1))
            : DateOnly.FromDateTime(subscription.NextBillingDate.UtcDateTime.Date.AddYears(1).AddDays(-1));

        return new BillingPricingResult(
            lineItems,
            tax,
            DateOnly.FromDateTime(subscription.NextBillingDate.UtcDateTime.Date),
            periodEnd,
            $"package:{package.Code}");
    }

    private static Money ResolvePackagePrice(string? metadataJson, BillingCycle billingCycle)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
            throw new DomainException("Commercial package metadata is required to resolve pricing.");

        try
        {
            using var document = JsonDocument.Parse(metadataJson);
            if (!document.RootElement.TryGetProperty("pricing", out var pricing) || pricing.ValueKind != JsonValueKind.Object)
                throw new DomainException("Commercial package metadata must contain a pricing object.");

            var cycleKey = billingCycle == BillingCycle.Monthly ? "monthly" : "annual";
            if (!pricing.TryGetProperty(cycleKey, out var cyclePricing) || cyclePricing.ValueKind != JsonValueKind.Object)
                throw new DomainException($"Commercial package pricing is missing the '{cycleKey}' entry.");

            if (!cyclePricing.TryGetProperty("amount", out var amountElement) || !amountElement.TryGetDecimal(out var amount))
                throw new DomainException($"Commercial package pricing for '{cycleKey}' must include a numeric amount.");

            var currency = cyclePricing.TryGetProperty("currency", out var currencyElement) && currencyElement.ValueKind == JsonValueKind.String
                ? currencyElement.GetString() ?? DefaultCurrency
                : DefaultCurrency;

            return new Money(decimal.Round(amount, 4), currency);
        }
        catch (JsonException ex)
        {
            throw new DomainException($"Commercial package metadata contains invalid pricing JSON: {ex.Message}");
        }
    }

    private static decimal ResolveTaxRate(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
            return 0.10m;

        try
        {
            using var document = JsonDocument.Parse(metadataJson);
            if (document.RootElement.TryGetProperty("taxRate", out var taxRateElement) && taxRateElement.TryGetDecimal(out var taxRate))
                return taxRate;
        }
        catch (JsonException)
        {
            return 0.10m;
        }

        return 0.10m;
    }
}
