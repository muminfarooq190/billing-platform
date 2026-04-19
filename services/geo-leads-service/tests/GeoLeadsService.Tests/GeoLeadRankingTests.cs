using FluentAssertions;
using GeoLeadsService.Application;
using GeoLeadsService.Domain.Aggregates;
using Xunit;

namespace GeoLeadsService.Tests;

public sealed class GeoLeadRankingTests
{
    [Fact]
    public void Score_ShouldFavorContactability_WhenModeIsContactability()
    {
        var tourismHeavy = BuildLead("Tourism Heavy", confidence: 0.80m, contactability: 0.40m, tourism: 0.95m);
        var contactHeavy = BuildLead("Contact Heavy", confidence: 0.75m, contactability: 0.95m, tourism: 0.60m);

        var tourismScore = GeoLeadRanking.Score(tourismHeavy, "contactability");
        var contactScore = GeoLeadRanking.Score(contactHeavy, "contactability");

        contactScore.Should().BeGreaterThan(tourismScore);
    }

    [Fact]
    public void Score_ShouldFavorPopularity_WhenModeIsPopularity()
    {
        var tourismHeavy = BuildLead("Tourism Heavy", confidence: 0.80m, contactability: 0.40m, tourism: 0.95m);
        var contactHeavy = BuildLead("Contact Heavy", confidence: 0.75m, contactability: 0.95m, tourism: 0.60m);

        var tourismScore = GeoLeadRanking.Score(tourismHeavy, "popularity");
        var contactScore = GeoLeadRanking.Score(contactHeavy, "popularity");

        tourismScore.Should().BeGreaterThan(contactScore);
    }

    [Theory]
    [InlineData(null, "relevance")]
    [InlineData("", "relevance")]
    [InlineData("relevance", "relevance")]
    [InlineData("contactability", "contactability")]
    [InlineData("popularity", "popularity")]
    [InlineData("weird-mode", "relevance")]
    public void NormalizeMode_ShouldNormalizeExpectedValues(string? input, string expected)
    {
        GeoLeadRanking.NormalizeMode(input).Should().Be(expected);
    }

    private static GeoLead BuildLead(string name, decimal confidence, decimal contactability, decimal tourism)
        => new(Guid.NewGuid(), name, "hotel", null, null, null, "Somewhere", 18.93m, 72.83m, "Mumbai", "Maharashtra", "India", confidence, contactability, tourism, ["stub"], ["reason"], DateTimeOffset.UtcNow);
}
