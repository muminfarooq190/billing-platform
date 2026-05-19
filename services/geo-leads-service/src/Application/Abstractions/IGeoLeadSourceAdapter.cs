namespace GeoLeadsService.Application.Abstractions;

/// <summary>
/// Source adapter for raw lead records. Adapters that can scope their fetch
/// to a geographic area (Overpass, Google Places, etc.) should honor the
/// optional <paramref name="boundingBox"/> hint; adapters with static data
/// (seeded, appsettings JSON) may ignore it and always return everything.
/// </summary>
public interface IGeoLeadSourceAdapter
{
    string SourceName { get; }

    /// <summary>
    /// Fetch raw records. If <paramref name="boundingBox"/> is supplied,
    /// implementations SHOULD restrict the fetch to records inside it.
    /// </summary>
    Task<IReadOnlyList<GeoLeadSourceRecordInput>> FetchAsync(
        CancellationToken cancellationToken,
        GeoBoundingBox? boundingBox = null);
}

public sealed record GeoLeadSourceRecordInput(
    string SourceRecordId,
    string RawName,
    string RawCategory,
    string? RawAddress,
    string? RawPhone,
    string? RawEmail,
    string? RawWebsite,
    decimal? RawLatitude,
    decimal? RawLongitude,
    string RawPayloadJson);

/// <summary>Geographic bounding box in lng/lat (decimal degrees).</summary>
public sealed record GeoBoundingBox(decimal MinLongitude, decimal MinLatitude, decimal MaxLongitude, decimal MaxLatitude)
{
    public static GeoBoundingBox FromPolygon(GeoPolygon polygon)
    {
        decimal minLng = decimal.MaxValue, minLat = decimal.MaxValue, maxLng = decimal.MinValue, maxLat = decimal.MinValue;
        foreach (var c in polygon.Coordinates)
        {
            if (c.Longitude < minLng) minLng = c.Longitude;
            if (c.Longitude > maxLng) maxLng = c.Longitude;
            if (c.Latitude < minLat) minLat = c.Latitude;
            if (c.Latitude > maxLat) maxLat = c.Latitude;
        }
        return new GeoBoundingBox(minLng, minLat, maxLng, maxLat);
    }
}
