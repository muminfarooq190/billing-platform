using FluentAssertions;
using TravelService.Application.Queries.GetItineraryById;

namespace TravelService.Tests;

public sealed class ItineraryOwnershipReadModelTests
{
    [Fact]
    public void BookingOwnedReadModel_ShouldExposeOwnershipFlags()
    {
        var model = new ItineraryReadModel(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Jane Doe",
            "Confirmed Plan",
            "Bali",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(5),
            2,
            "USD",
            Guid.NewGuid(),
            Guid.NewGuid(),
            true,
            "Booking",
            "Draft",
            0m,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);

        model.IsBookingOwned.Should().BeTrue();
        model.OwnershipType.Should().Be("Booking");
    }

    [Fact]
    public void LegacyQuotationReadModel_ShouldExposeLegacyOwnershipFlags()
    {
        var model = new ItineraryReadModel(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Jane Doe",
            "Legacy Draft",
            "Bali",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(5),
            2,
            "USD",
            Guid.NewGuid(),
            null,
            false,
            "QuotationLegacy",
            "Draft",
            0m,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);

        model.IsBookingOwned.Should().BeFalse();
        model.OwnershipType.Should().Be("QuotationLegacy");
    }
}
