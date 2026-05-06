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
        var model = new BookingReadModel
        {
            Id = bookingId,
            TenantId = tenantId,
            QuotationId = Guid.NewGuid(),
            AcceptedRevisionId = Guid.NewGuid(),
            PrimaryContactId = contactId,
            BookingNumber = "VOY-BKG-2026-000001",
            Status = "Pending",
            TripName = "Rome Trip",
            Destination = "Rome",
            StartDate = DateTimeOffset.UtcNow.AddDays(10),
            EndDate = DateTimeOffset.UtcNow.AddDays(15),
            TravellersCount = 2,
            Currency = "USD",
            TotalSellAmount = 2500m,
            TotalPaidAmount = 0m,
            TotalOutstandingAmount = 2500m,
            TotalCostAmount = 1800m,
            MarginAmount = 700m,
            CustomerName = "Jane Doe",
            TravelersRequired = 2,
            TravelersComplete = 1,
            DocumentsRequired = 2,
            DocumentsUploaded = 1,
            ItineraryId = itineraryId,
            HasItinerary = true,
            ItineraryStatus = "Draft",
            ItineraryUpdatedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CancelledAt = null
        };

        model.BookingNumber.Should().Be("VOY-BKG-2026-000001");
        model.Status.Should().Be("Pending");
        model.PrimaryContactId.Should().Be(contactId);
        model.TotalSellAmount.Should().Be(2500m);
        model.TotalPaidAmount.Should().Be(0m);
        model.TotalOutstandingAmount.Should().Be(2500m);
        model.CustomerName.Should().Be("Jane Doe");
        model.TravelersRequired.Should().Be(2);
        model.TravelersComplete.Should().Be(1);
        model.DocumentsRequired.Should().Be(2);
        model.DocumentsUploaded.Should().Be(1);
        model.MarginAmount.Should().Be(700m);
        model.ItineraryId.Should().Be(itineraryId);
        model.HasItinerary.Should().BeTrue();
        model.ItineraryStatus.Should().Be("Draft");
    }
}
