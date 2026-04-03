using BillingService.Domain.Exceptions;
using BillingService.Domain.ValueObjects;
using FluentAssertions;

namespace BillingService.Tests.Domain;

public sealed class MoneyTests
{
    [Fact]
    public void Add_DifferentCurrency_ShouldThrow()
    {
        var usd = new Money(10m, "USD");
        var eur = new Money(2m, "EUR");

        var action = () => usd.Add(eur);
        action.Should().Throw<DomainException>();
    }
}
