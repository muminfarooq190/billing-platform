using TravelService.Domain.Exceptions;

namespace TravelService.Domain.ValueObjects;

public sealed record QuotationLineItem
{
    public QuotationLineItem(string description, decimal unitPrice, int quantity, string currency)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Quotation line item description is required.");
        if (unitPrice < 0)
            throw new DomainException("Quotation line item unit price cannot be negative.");
        if (quantity <= 0)
            throw new DomainException("Quotation line item quantity must be greater than zero.");
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Quotation line item currency is required.");

        Description = description.Trim();
        UnitPrice = unitPrice;
        Quantity = quantity;
        Currency = currency.Trim().ToUpperInvariant();
    }

    private QuotationLineItem() { }

    public string Description { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public string Currency { get; init; } = string.Empty;
    public decimal Total => UnitPrice * Quantity;
}
