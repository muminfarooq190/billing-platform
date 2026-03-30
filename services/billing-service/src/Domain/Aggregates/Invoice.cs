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

    private Invoice(Guid subscriptionId, Guid tenantId, IReadOnlyCollection<InvoiceLineItem> lineItems, Money taxAmount, DateTimeOffset dueDate)
    {
        Id = Guid.NewGuid();
        SubscriptionId = subscriptionId;
        TenantId = tenantId;
        _lineItems.AddRange(lineItems);
        Subtotal = _lineItems.Select(x => x.LineTotal).Aggregate(new Money(0m, taxAmount.Currency), (acc, x) => acc.Add(x));
        TaxAmount = taxAmount;
        Total = Subtotal.Add(TaxAmount);
        Status = InvoiceStatus.Issued;
        DueDate = dueDate;
        IssuedAt = DateTimeOffset.UtcNow;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new InvoiceCreatedEvent(Id, TenantId));
    }

    public Guid Id { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public Guid TenantId { get; private set; }
    public IReadOnlyList<InvoiceLineItem> LineItems => _lineItems;
    public Money Subtotal { get; private set; }
    public Money TaxAmount { get; private set; }
    public Money Total { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateTimeOffset DueDate { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public DateTimeOffset? IssuedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static Invoice Generate(Guid subscriptionId, Guid tenantId, IReadOnlyCollection<InvoiceLineItem> lineItems, Money taxAmount, DateTimeOffset dueDate)
        => new(subscriptionId, tenantId, lineItems, taxAmount, dueDate);

    public void MarkAsPaid(DateTimeOffset paidAt)
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
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new InvoicePaidEvent(Id, TenantId));
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
}
