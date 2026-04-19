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

    [Fact]
    public void SavedGeoArea_ShouldUpdateNameGeometryAndTimestamp()
    {
        var area = new SavedGeoArea(Guid.NewGuid(), "Old Name", "{\"old\":true}");
        var originalUpdatedAt = area.UpdatedAt;

        Thread.Sleep(5);
        area.Update("  New Name  ", "{\"new\":true}");

        area.Name.Should().Be("New Name");
        area.GeometryJson.Should().Be("{\"new\":true}");
        area.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }
}
