using GeoLeadsService.Application.Abstractions;
using NetTopologySuite.Geometries;

namespace GeoLeadsService.Infrastructure.Persistence.Spatial;

public static class GeoSpatialConversions
{
    private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 4326);

    public static Polygon ToPolygon(GeoPolygon polygon)
    {
        var coordinates = polygon.Coordinates
            .Select(x => new Coordinate((double)x.Longitude, (double)x.Latitude))
            .ToArray();

        return GeometryFactory.CreatePolygon(coordinates);
    }

    public static Polygon? TryToPolygon(GeoPolygon? polygon)
    {
        if (polygon?.Coordinates is null || polygon.Coordinates.Count < 4)
            return null;

        return ToPolygon(polygon);
    }

    public static Point ToPoint(decimal longitude, decimal latitude)
        => GeometryFactory.CreatePoint(new Coordinate((double)longitude, (double)latitude));
}
