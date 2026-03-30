namespace BillingService.Domain.ValueObjects;

public sealed record InvoiceLineItem(string Description, int Quantity, Money UnitPrice)
{
    public Money LineTotal => new(decimal.Round(UnitPrice.Amount * Quantity, 4), UnitPrice.Currency);
}
