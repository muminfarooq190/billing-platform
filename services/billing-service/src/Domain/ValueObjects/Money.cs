using BillingService.Domain.Exceptions;

namespace BillingService.Domain.ValueObjects;

public readonly record struct Money(decimal Amount, string Currency)
{
    public Money Add(Money other)
    {
        GuardCurrency(other);
        return new Money(decimal.Round(Amount + other.Amount, 4), Currency);
    }

    public Money Subtract(Money other)
    {
        GuardCurrency(other);
        return new Money(decimal.Round(Amount - other.Amount, 4), Currency);
    }

    private void GuardCurrency(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("Cross-currency arithmetic is not allowed.");
        }
    }
}
