using FluentAssertions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;

namespace TravelService.Tests;

public sealed class DraftTripConceptCoreTests
{
    [Fact]
    public void CreateConcept_ShouldStartAsDraft_AndAllowDays()
    {
        var concept = DraftTripConcept.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Bali Honeymoon Option A",
            "Bali",
            "Beach + Ubud split",
            DateTimeOffset.UtcNow.AddDays(30),
            DateTimeOffset.UtcNow.AddDays(36),
            2,
            "INR",
            150000m,
            "Option A",
            "Premium positioning",
            Guid.NewGuid());

        concept.AddDay(1, "Arrival", "Airport pickup and check-in", "Bali", "Seminyak");

        concept.ConceptStatus.Should().Be(DraftTripConceptStatus.Draft);
        concept.Days.Should().ContainSingle();
        concept.IsPrimary.Should().BeFalse();
    }

    [Fact]
    public void MarkPrimary_AndReadyForQuote_ShouldUpdateState()
    {
        var concept = DraftTripConcept.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Bali Honeymoon Option A",
            "Bali",
            null,
            null,
            null,
            2,
            "USD",
            null,
            null,
            null,
            null);

        concept.MarkPrimary();
        concept.MarkReadyForQuote();

        concept.IsPrimary.Should().BeTrue();
        concept.ConceptStatus.Should().Be(DraftTripConceptStatus.ReadyForQuote);
    }

    [Fact]
    public void Archive_ShouldClearPrimary_AndMoveToArchived()
    {
        var concept = DraftTripConcept.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Bali Honeymoon Option A",
            "Bali",
            null,
            null,
            null,
            2,
            "USD",
            null,
            null,
            null,
            null);

        concept.MarkPrimary();
        concept.Archive();

        concept.IsPrimary.Should().BeFalse();
        concept.ConceptStatus.Should().Be(DraftTripConceptStatus.Archived);
    }
}
