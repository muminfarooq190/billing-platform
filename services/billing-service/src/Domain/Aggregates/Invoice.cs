using System.Globalization;
using BillingService.Domain.Common;
using BillingService.Domain.Enums;
using BillingService.Domain.Events;
using BillingService.Domain.Exceptions;
using BillingService.Domain.ValueObjects;

namespace BillingService.Domain.Aggregates;

public sealed class Invoice : AggregateRoot
{
    private readonly List<InvoiceLineItem> _lineItems = [];

    private Invoice() { }

    private Invoice(
        Guid subscriptionId,
        Guid tenantId,
        IReadOnlyCollection<InvoiceLineItem> lineItems,
        Money taxAmount,
        DateTimeOffset dueDate,
        DateOnly billingPeriodStart,
        DateOnly billingPeriodEnd,
        string pricingReference)
    {
        Id = Guid.NewGuid();
        SubscriptionId = subscriptionId;
        TenantId = tenantId;
        _lineItems.AddRange(lineItems);
        InvoiceNumber = GenerateInvoiceNumber(tenantId);
        Subtotal = _lineItems.Select(x => x.LineTotal).Aggregate(new Money(0m, taxAmount.Currency), (acc, x) => acc.Add(x));
        TaxAmount = taxAmount;
        Total = Subtotal.Add(TaxAmount);
        Status = InvoiceStatus.Issued;
        DueDate = dueDate;
        BillingPeriodStart = billingPeriodStart;
        BillingPeriodEnd = billingPeriodEnd;
        PricingReference = pricingReference.Trim();
        IssuedAt = DateTimeOffset.UtcNow;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new InvoiceCreatedEvent(
            Id,
            TenantId,
            SubscriptionId,
            Status.ToString(),
            Subtotal.Amount,
            Subtotal.Currency,
            TaxAmount.Amount,
            Total.Amount,
            DueDate,
            BillingPeriodStart,
            BillingPeriodEnd,
            PricingReference,
            _lineItems.Select(x => new InvoiceCreatedLineItem(x.Description, x.Quantity, x.UnitPrice.Amount, x.UnitPrice.Currency, x.LineTotal.Amount)).ToList()));
    }

    public Guid Id { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public Guid TenantId { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public IReadOnlyList<InvoiceLineItem> LineItems => _lineItems;
    public Money Subtotal { get; private set; }
    public Money TaxAmount { get; private set; }
    public Money Total { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateTimeOffset DueDate { get; private set; }
    public DateOnly BillingPeriodStart { get; private set; }
    public DateOnly BillingPeriodEnd { get; private set; }
    public string PricingReference { get; private set; } = string.Empty;
    public string? ProviderPaymentId { get; private set; }
    public string? PaymentGateway { get; private set; }
    public string? PaymentFailureCode { get; private set; }
    public string? PaymentFailureMessage { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public DateTimeOffset? IssuedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static Invoice Generate(Guid subscriptionId, Guid tenantId, IReadOnlyCollection<InvoiceLineItem> lineItems, Money taxAmount, DateTimeOffset dueDate, DateOnly billingPeriodStart, DateOnly billingPeriodEnd, string pricingReference)
        => new(subscriptionId, tenantId, lineItems, taxAmount, dueDate, billingPeriodStart, billingPeriodEnd, pricingReference);

    public void MarkPaymentPending(string gateway, string providerPaymentId)
    {
        PaymentGateway = gateway?.Trim();
        ProviderPaymentId = providerPaymentId?.Trim();
        PaymentFailureCode = null;
        PaymentFailureMessage = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkPaymentFailed(string gateway, string? errorCode, string? errorMessage)
    {
        PaymentGateway = gateway?.Trim();
        PaymentFailureCode = errorCode?.Trim();
        PaymentFailureMessage = errorMessage?.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsPaid(DateTimeOffset paidAt, string? gateway = null, string? providerPaymentId = null)
    {
        if (Status == InvoiceStatus.Paid)
        {
            return;
        }

        if (Status == InvoiceStatus.Void)
        {
            throw new DomainException("Void invoice cannot be paid.");
        }

        Status = InvoiceStatus.Paid;
        PaidAt = paidAt;
        PaymentGateway = gateway?.Trim() ?? PaymentGateway;
        ProviderPaymentId = providerPaymentId?.Trim() ?? ProviderPaymentId;
        PaymentFailureCode = null;
        PaymentFailureMessage = null;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new InvoicePaidEvent(
            Id,
            TenantId,
            SubscriptionId,
            Status.ToString(),
            Total.Amount,
            Total.Currency,
            PaidAt.Value,
            PaymentGateway,
            ProviderPaymentId,
            PricingReference));
    }

    public void MarkOverdue()
    {
        if (Status is InvoiceStatus.Paid or InvoiceStatus.Void)
        {
            return;
        }

        Status = InvoiceStatus.Overdue;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string GenerateInvoiceNumber(Guid tenantId)
    {
        var tenantPrefix = tenantId.ToString("N", CultureInfo.InvariantCulture)[..6].ToUpperInvariant();
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        return $"INV-{tenantPrefix}-{timestamp}";
    }
}
