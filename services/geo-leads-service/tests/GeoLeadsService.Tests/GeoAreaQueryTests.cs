using FluentAssertions;
using GeoLeadsService.Domain.Aggregates;
using Xunit;

namespace GeoLeadsService.Tests;

public sealed class GeoAreaQueryTests
{
    [Fact]
    public void GeoAreaQuery_ShouldDefaultRankingModeToRelevance()
    {
        var query = new GeoAreaQuery(Guid.NewGuid(), "{}", [], 10, null);

        query.RankingMode.Should().Be("relevance");
    }
}
