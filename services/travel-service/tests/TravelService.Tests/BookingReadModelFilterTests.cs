using FluentAssertions;
using TravelService.Application.Queries.GetBookingById;
using TravelService.Application.Queries.ListBookings;

namespace TravelService.Tests;

public sealed class BookingReadModelFilterTests
{
    [Fact]
    public void ListBookingsQuery_ShouldCaptureFilterValues()
    {
        var assignedToUserId = Guid.NewGuid();
        var primaryContactId = Guid.NewGuid();
        var query = new ListBookingsQuery(
            Guid.NewGuid(),
            Page: 2,
            PageSize: 50,
            Status: "Pending",
            Destination: "Rome",
            StartDateFrom: DateTimeOffset.UtcNow.Date,
            StartDateTo: DateTimeOffset.UtcNow.Date.AddDays(30),
            AssignedToUserId: assignedToUserId,
            PrimaryContactId: primaryContactId);

        query.Page.Should().Be(2);
        query.PageSize.Should().Be(50);
        query.Status.Should().Be("Pending");
        query.Destination.Should().Be("Rome");
        query.AssignedToUserId.Should().Be(assignedToUserId);
        query.PrimaryContactId.Should().Be(primaryContactId);
    }

    [Fact]
    public void BookingReadModel_ShouldExposeCoreFields()
    {
        var bookingId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var itineraryId = Guid.NewGuid();
        var model = new BookingReadModel(
            bookingId,
            tenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            contactId,
            "VOY-BKG-2026-000001",
            "Pending",
            "Rome Trip",
            "Rome",
            DateTimeOffset.UtcNow.AddDays(10),
            DateTimeOffset.UtcNow.AddDays(15),
            2,
            "USD",
            2500m,
            1800m,
            700m,
            null,
            null,
            "Priority booking",
            itineraryId,
            true,
            "Draft",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            null);

        model.BookingNumber.Should().Be("VOY-BKG-2026-000001");
        model.Status.Should().Be("Pending");
        model.PrimaryContactId.Should().Be(contactId);
        model.TotalSellAmount.Should().Be(2500m);
        model.MarginAmount.Should().Be(700m);
        model.ItineraryId.Should().Be(itineraryId);
        model.HasItinerary.Should().BeTrue();
        model.ItineraryStatus.Should().Be("Draft");
    }
}
