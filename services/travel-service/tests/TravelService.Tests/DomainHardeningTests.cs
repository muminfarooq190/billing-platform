using FluentAssertions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;
using TravelService.Domain.Exceptions;

namespace TravelService.Tests;

public sealed class DomainHardeningTests
{
    [Fact]
    public void Quotation_Send_ShouldRequireLineItems()
    {
        var quotation = CreateDraftQuotation();

        var act = () => quotation.Send();

        act.Should().Throw<DomainException>().WithMessage("*no line items*");
    }

    [Fact]
    public void Quotation_Update_ShouldRejectCurrencyChange_WhenLineItemsAlreadyExist()
    {
        var quotation = CreateDraftQuotation();
        quotation.AddLineItem("Flight", 250m, 2, "USD");

        var act = () => quotation.Update("Summer Trip", "Istanbul", DateTimeOffset.UtcNow.AddDays(20), DateTimeOffset.UtcNow.AddDays(25), 2, "EUR", "notes", DateTimeOffset.UtcNow.AddDays(10));

        act.Should().Throw<DomainException>().WithMessage("*currency*");
    }

    [Fact]
    public void Quotation_CreateRevision_ShouldIncrementVersion_AndPreserveSnapshotTotals()
    {
        var quotation = CreateDraftQuotation();
        quotation.AddLineItem("Flight", 250m, 2, "USD");
        quotation.AddLineItem("Hotel", 500m, 1, "USD");

        var revision = quotation.CreateRevision("Customer notes", "Internal notes");

        quotation.CurrentRevisionNumber.Should().Be(1);
        revision.RevisionNumber.Should().Be(1);
        revision.TotalAmount.Should().Be(1000m);
        revision.LineItems.Should().HaveCount(2);
        revision.VisibleNotes.Should().Be("Customer notes");
        revision.InternalNotes.Should().Be("Internal notes");
    }

    [Fact]
    public void Itinerary_Confirm_ShouldRequireItems()
    {
        var itinerary = Itinerary.Create(Guid.NewGuid(), Guid.NewGuid(), "Ava", "Trip", "Dubai", DateTimeOffset.UtcNow.AddDays(5), DateTimeOffset.UtcNow.AddDays(8), 2, "USD", null);

        var act = () => itinerary.Confirm();

        act.Should().Throw<DomainException>().WithMessage("*no scheduled items*");
    }

    [Fact]
    public void Itinerary_AddItem_ShouldRejectDifferentCurrency()
    {
        var itinerary = Itinerary.Create(Guid.NewGuid(), Guid.NewGuid(), "Ava", "Trip", "Dubai", DateTimeOffset.UtcNow.AddDays(5), DateTimeOffset.UtcNow.AddDays(8), 2, "USD", null);
        itinerary.AddItem(1, ItineraryItemType.Flight, "Outbound", "Flight", "DXB", null, null, 400m, "USD");

        var act = () => itinerary.AddItem(2, ItineraryItemType.Hotel, "Hotel", "Stay", "Dubai Marina", null, null, 500m, "EUR");

        act.Should().Throw<DomainException>().WithMessage("*currency must match*");
    }

    [Fact]
    public void FollowUp_Update_ShouldRejectPastDueDate()
    {
        var followUp = FollowUp.Create(Guid.NewGuid(), Guid.NewGuid(), "Ava", "Call customer", "notes", FollowUpPriority.High, DateTimeOffset.UtcNow.AddDays(1), null);

        var act = () => followUp.Update("Call customer", "notes", FollowUpPriority.High, DateTimeOffset.UtcNow.AddDays(-1), null);

        act.Should().Throw<DomainException>().WithMessage("*past*");
    }

    private static Quotation CreateDraftQuotation()
        => Quotation.Create(Guid.NewGuid(), Guid.NewGuid(), "Ava", "Summer Trip", "Istanbul", DateTimeOffset.UtcNow.AddDays(20), DateTimeOffset.UtcNow.AddDays(25), 2, "USD", "notes");
}
