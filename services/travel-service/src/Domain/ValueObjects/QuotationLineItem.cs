namespace TravelService.Domain.ValueObjects;

public sealed record QuotationLineItem(string Description, decimal UnitPrice, int Quantity, string Currency)
{
    public decimal Total => UnitPrice * Quantity;
}
