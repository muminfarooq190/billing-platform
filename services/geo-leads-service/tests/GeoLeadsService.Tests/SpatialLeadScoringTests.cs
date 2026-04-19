using FluentAssertions;
using GeoLeadsService.Application;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;
using Xunit;

namespace GeoLeadsService.Tests;

public sealed class SpatialLeadScoringTests
{
    [Fact]
    public void Rank_ShouldPreferCloserLead_WhenBaseScoresAreComparable()
    {
        var polygon = new GeoPolygon([
            new GeoCoordinate(72.82m, 18.92m),
            new GeoCoordinate(72.84m, 18.92m),
            new GeoCoordinate(72.84m, 18.94m),
            new GeoCoordinate(72.82m, 18.94m),
            new GeoCoordinate(72.82m, 18.92m)
        ]);

        var closeLead = BuildLead("Close Lead", 18.93m, 72.83m);
        var fartherLead = BuildLead("Farther Lead", 18.99m, 72.89m);

        var ranked = SpatialLeadScoring.Rank(polygon, [fartherLead, closeLead], "relevance");

        ranked[0].Lead.CanonicalName.Should().Be("Close Lead");
        ranked[0].DistanceMeters.Should().BeLessThan(ranked[1].DistanceMeters);
    }

    private static GeoLead BuildLead(string name, decimal lat, decimal lng)
        => new(Guid.NewGuid(), name, "hotel", null, null, null, "Somewhere", lat, lng, "Mumbai", "Maharashtra", "India", 0.8m, 0.8m, 0.8m, ["stub"], ["reason"], DateTimeOffset.UtcNow);
}
