using FluentAssertions;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Infrastructure.Persistence.Repositories;
using Xunit;

namespace GeoLeadsService.Tests;

public sealed class GeoLeadCatalogTests
{
    [Fact]
    public async Task SeededGeoLeadCatalog_ShouldReturnLeadsInsidePolygon()
    {
        var catalog = new SeededGeoLeadCatalog();
        var polygon = new GeoPolygon(
        [
            new GeoCoordinate(72.82m, 18.92m),
            new GeoCoordinate(72.84m, 18.92m),
            new GeoCoordinate(72.84m, 18.94m),
            new GeoCoordinate(72.82m, 18.94m)
        ]);

        var results = await catalog.SearchAsync(polygon, [], 10, CancellationToken.None);

        results.Should().NotBeEmpty();
        results.Should().Contain(x => x.CanonicalName == "Sunrise Adventures");
    }
}
