using FluentAssertions;
using GeoLeadsService.Api.Contracts;
using Xunit;

namespace GeoLeadsService.Tests;

public sealed class GeometryPayloadTests
{
    [Fact]
    public void GeometryPayload_ShouldRejectOpenPolygon()
    {
        var payload = new GeometryPayload("Polygon",
        [
            [72.82m, 18.92m],
            [72.84m, 18.92m],
            [72.84m, 18.94m],
            [72.82m, 18.94m]
        ]);

        var result = payload.IsValidPolygon(out var error);

        result.Should().BeFalse();
        error.Should().Contain("closed");
    }
}
