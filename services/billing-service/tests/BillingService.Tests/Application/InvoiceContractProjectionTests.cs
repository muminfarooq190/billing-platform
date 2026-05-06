using BillingService.Application.ReadModels;
using BillingService.Domain.Aggregates;
using BillingService.Domain.ValueObjects;
using FluentAssertions;

namespace BillingService.Tests.Application;

public sealed class InvoiceContractProjectionTests
{
    [Fact]
    public void Generate_ShouldPopulateInvoiceNumber()
    {
        var invoice = Invoice.Generate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [new InvoiceLineItem("Service", 1, new Money(100m, "USD"))],
            new Money(0m, "USD"),
            DateTimeOffset.UtcNow.AddDays(14),
            DateOnly.FromDateTime(DateTime.UtcNow.Date),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(30)),
            "pricing-ref");

        invoice.InvoiceNumber.Should().StartWith("INV-");
    }

    [Fact]
    public void ReadModel_ShouldSupportFlattenedInvoiceAmounts()
    {
        var model = new InvoiceReadModel
        {
            InvoiceNumber = "INV-TEST-1",
            TotalAmount = 125m,
            PaidAmount = 0m,
            DueAmount = 125m,
            Currency = "USD"
        };

        model.InvoiceNumber.Should().Be("INV-TEST-1");
        model.TotalAmount.Should().Be(125m);
        model.PaidAmount.Should().Be(0m);
        model.DueAmount.Should().Be(125m);
        model.Currency.Should().Be("USD");
    }
}
