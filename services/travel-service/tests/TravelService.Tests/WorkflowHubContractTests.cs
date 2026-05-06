using FluentAssertions;
using TravelService.Application.Queries.GetWorkQueue;

namespace TravelService.Tests;

public sealed class WorkflowHubContractTests
{
    [Fact]
    public void WorkflowHubItemReadModel_ShouldExposeFrontendFields()
    {
        var item = new WorkflowHubItemReadModel(
            Guid.NewGuid(),
            "Qualified",
            "Jane Doe",
            "jane@example.com",
            "+123456789",
            "Rome",
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddDays(7),
            2,
            5000m,
            "USD",
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Sent",
            Guid.NewGuid(),
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Confirmed",
            Guid.NewGuid(),
            "Draft",
            DateTime.UtcNow);

        item.FullName.Should().Be("Jane Doe");
        item.QuotationStatus.Should().Be("Sent");
        item.BookingStatus.Should().Be("Confirmed");
        item.ItineraryStatus.Should().Be("Draft");
        item.BudgetCurrency.Should().Be("USD");
    }
}
