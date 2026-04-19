using FluentAssertions;
using GeoLeadsService.Domain.Aggregates;
using Xunit;

namespace GeoLeadsService.Tests;

public sealed class SavedGeoAreaTests
{
    [Fact]
    public void SavedGeoArea_ShouldTrimName()
    {
        var area = new SavedGeoArea(Guid.NewGuid(), "  South Mumbai  ", "{}");

        area.Name.Should().Be("South Mumbai");
    }
}
