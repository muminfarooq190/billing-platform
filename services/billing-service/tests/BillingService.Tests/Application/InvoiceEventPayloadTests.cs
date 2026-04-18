using BillingService.Domain.Aggregates;
using BillingService.Domain.Events;
using BillingService.Domain.ValueObjects;
using FluentAssertions;

namespace BillingService.Tests.Application;

public sealed class InvoiceEventPayloadTests
{
    [Fact]
    public void InvoiceGenerate_ShouldRaiseRichInvoiceCreatedEvent()
    {
        var invoice = Invoice.Generate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [new InvoiceLineItem("Growth package", 1, new Money(129m, "USD"))],
            new Money(12.9m, "USD"),
            DateTimeOffset.UtcNow.AddDays(14),
            new DateOnly(2026, 4, 1),
            new DateOnly(2026, 4, 30),
            "package:growth.v1");

        var domainEvent = invoice.DomainEvents.OfType<InvoiceCreatedEvent>().Single();

        domainEvent.TotalAmount.Should().Be(141.9m);
        domainEvent.Currency.Should().Be("USD");
        domainEvent.PricingReference.Should().Be("package:growth.v1");
        domainEvent.LineItems.Should().ContainSingle();
        domainEvent.LineItems[0].Description.Should().Be("Growth package");
    }

    [Fact]
    public void InvoiceMarkAsPaid_ShouldRaiseRichInvoicePaidEvent()
    {
        var invoice = Invoice.Generate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [new InvoiceLineItem("Growth package", 1, new Money(129m, "USD"))],
            new Money(12.9m, "USD"),
            DateTimeOffset.UtcNow.AddDays(14),
            new DateOnly(2026, 4, 1),
            new DateOnly(2026, 4, 30),
            "package:growth.v1");
        invoice.ClearDomainEvents();

        invoice.MarkAsPaid(DateTimeOffset.UtcNow, "Stripe", "cs_test_123");

        var domainEvent = invoice.DomainEvents.OfType<InvoicePaidEvent>().Single();
        domainEvent.TotalAmount.Should().Be(141.9m);
        domainEvent.Currency.Should().Be("USD");
        domainEvent.PaymentGateway.Should().Be("Stripe");
        domainEvent.ProviderPaymentId.Should().Be("cs_test_123");
        domainEvent.PricingReference.Should().Be("package:growth.v1");
    }
}
