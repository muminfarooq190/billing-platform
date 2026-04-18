namespace GeoLeadsService.Api.Contracts;

public sealed record SubmitGeoAreaQueryRequest(
    GeometryPayload Geometry,
    IReadOnlyList<string>? LeadTypes,
    int? Limit,
    string? RankingMode);

public sealed record GeometryPayload(string Type, IReadOnlyList<IReadOnlyList<decimal>> Coordinates)
{
    public bool IsValidPolygon(out string? error)
    {
        error = null;
        if (!string.Equals(Type, "Polygon", StringComparison.OrdinalIgnoreCase))
        {
            error = "Only Polygon geometry is supported in the initial MVP implementation.";
            return false;
        }

        if (Coordinates is null || Coordinates.Count < 4)
        {
            error = "Polygon must contain at least four coordinate pairs including closure point.";
            return false;
        }

        if (Coordinates.Any(x => x.Count != 2))
        {
            error = "Each coordinate must contain [longitude, latitude].";
            return false;
        }

        var first = Coordinates.First();
        var last = Coordinates.Last();
        if (first[0] != last[0] || first[1] != last[1])
        {
            error = "Polygon must be closed by repeating the first coordinate as the last point.";
            return false;
        }

        foreach (var coordinate in Coordinates)
        {
            var lng = coordinate[0];
            var lat = coordinate[1];
            if (lng < -180 || lng > 180 || lat < -90 || lat > 90)
            {
                error = "Coordinates must be valid longitude/latitude values.";
                return false;
            }
        }

        return true;
    }
}

public sealed record GeoAreaQueryResponse(Guid QueryId, string Status, int Count);
