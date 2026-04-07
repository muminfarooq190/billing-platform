using TravelService.Domain.Exceptions;

namespace TravelService.Domain.Aggregates;

public sealed class QuotationRevisionLineItem
{
    private QuotationRevisionLineItem() { }

    private QuotationRevisionLineItem(string description, int quantity, decimal unitPriceAmount, string currency, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Line item description is required.");
        if (quantity <= 0)
            throw new DomainException("Line item quantity must be greater than zero.");
        if (unitPriceAmount < 0)
            throw new DomainException("Line item unit price cannot be negative.");
        if (sortOrder <= 0)
            throw new DomainException("Line item sort order must be greater than zero.");

        Id = Guid.NewGuid();
        Description = description.Trim();
        Quantity = quantity;
        UnitPriceAmount = unitPriceAmount;
        Currency = NormalizeCurrency(currency);
        SortOrder = sortOrder;
    }

    public Guid Id { get; private set; }
    public Guid QuotationRevisionId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPriceAmount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public int SortOrder { get; private set; }
    public decimal LineTotal => UnitPriceAmount * Quantity;

    public static QuotationRevisionLineItem Create(string description, int quantity, decimal unitPriceAmount, string currency, int sortOrder)
        => new(description, quantity, unitPriceAmount, currency, sortOrder);

    private static string NormalizeCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency is required.");

        var normalized = currency.Trim().ToUpperInvariant();
        if (normalized.Length != 3)
            throw new DomainException("Currency must be a 3-letter ISO code.");

        return normalized;
    }
}
