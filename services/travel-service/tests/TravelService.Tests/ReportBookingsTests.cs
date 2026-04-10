using FluentAssertions;
using TravelService.Application.Queries.ReportBookings;
using TravelService.Application.Queries.SearchTravel;

namespace TravelService.Tests;

public sealed class ReportBookingsTests
{
    [Fact]
    public void ReportBookingsQuery_ShouldCaptureFilters()
    {
        var query = new ReportBookingsQuery(Guid.NewGuid(), "Pending", "Italy");

        query.Status.Should().Be("Pending");
        query.Destination.Should().Be("Italy");
    }

    [Fact]
    public void SearchTravelQuery_ShouldCapturePagingAndSearchText()
    {
        var query = new SearchTravelQuery(Guid.NewGuid(), "Rome", 2, 50);

        query.Query.Should().Be("Rome");
        query.Page.Should().Be(2);
        query.PageSize.Should().Be(50);
    }

    [Fact]
    public void BookingReportRow_ShouldExposeExpectedFields()
    {
        var row = new BookingReportRow(Guid.NewGuid(), "VOY-BKG-1", "Italy Trip", "Italy", "Pending", "USD", 5000m, DateTimeOffset.UtcNow.AddDays(10), DateTimeOffset.UtcNow.AddDays(15), 2);

        row.BookingNumber.Should().Be("VOY-BKG-1");
        row.TotalSellAmount.Should().Be(5000m);
        row.Travellers.Should().Be(2);
    }
}
