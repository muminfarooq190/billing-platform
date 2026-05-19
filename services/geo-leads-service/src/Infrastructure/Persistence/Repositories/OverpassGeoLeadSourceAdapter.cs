using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GeoLeadsService.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GeoLeadsService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Adapter that pulls travel-domain POIs from the OpenStreetMap Overpass API.
///
/// Overpass is free, requires no API key, but rate-limits aggressive use —
/// caller is expected to scope ingestion to a bbox (passed via
/// <see cref="FetchAsync"/>'s boundingBox hint). Without a bbox the
/// adapter returns an empty list (instead of scraping the whole planet).
///
/// Categories ingested:
///   - tourism=hotel     → "hotel"
///   - tourism=guest_house, hostel → "hotel"
///   - shop=travel_agency → "tour_operator"
///   - office=travel_agent → "tour_operator"
///   - tourism=attraction, museum, viewpoint → "attraction"
///   - amenity=restaurant → "restaurant"
///   - tourism=information → "transport" (proxy)
///
/// Note: enabled by default. Disable via
/// `GeoLeadSources:Overpass:Enabled=false` if you need a hermetic test run.
/// </summary>
public sealed class OverpassGeoLeadSourceAdapter(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<OverpassGeoLeadSourceAdapter> logger) : IConfigurableGeoLeadSourceAdapter
{
    private const string DefaultEndpoint = "https://overpass-api.de/api/interpreter";
    private const int DefaultLimit = 200;

    public string SourceName => "openstreetmap-overpass";

    public bool IsEnabled => configuration.GetValue<bool?>("GeoLeadSources:Overpass:Enabled") ?? true;

    public async Task<IReadOnlyList<GeoLeadSourceRecordInput>> FetchAsync(
        CancellationToken cancellationToken,
        GeoBoundingBox? boundingBox = null)
    {
        if (boundingBox is null)
        {
            // Refuse to fetch the whole world. Overpass requires a bbox.
            return Array.Empty<GeoLeadSourceRecordInput>();
        }

        var endpoint = configuration["GeoLeadSources:Overpass:Endpoint"] ?? DefaultEndpoint;
        var limit = configuration.GetValue<int?>("GeoLeadSources:Overpass:MaxRecords") ?? DefaultLimit;
        var timeoutSec = configuration.GetValue<int?>("GeoLeadSources:Overpass:TimeoutSeconds") ?? 30;

        // Overpass QL — south, west, north, east order.
        var bbox = $"{boundingBox.MinLatitude:0.######},{boundingBox.MinLongitude:0.######},{boundingBox.MaxLatitude:0.######},{boundingBox.MaxLongitude:0.######}";
        var query = $"""
            [out:json][timeout:{timeoutSec}];
            (
              node["tourism"~"^(hotel|guest_house|hostel|attraction|museum|viewpoint|information)$"]({bbox});
              node["shop"="travel_agency"]({bbox});
              node["office"="travel_agent"]({bbox});
              node["amenity"="restaurant"]({bbox});
            );
            out body {limit};
            """;

        var client = httpClientFactory.CreateClient("overpass");
        client.Timeout = TimeSpan.FromSeconds(timeoutSec + 5);

        try
        {
            // Use GET with ?data= to sidestep Overpass's picky POST Content-Type rules
            // (their server rejects `application/x-www-form-urlencoded; charset=utf-8`
            // with 406 even though the body is identical).
            var url = endpoint + "?data=" + Uri.EscapeDataString(query);
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            // Overpass etiquette: identify the caller so operators can throttle by
            // UA instead of IP-banning. Note: the URL-style `(+https://…)` comment
            // suffix triggers their bot filter and returns 406. Plain UA only.
            request.Headers.UserAgent.ParseAdd("voyara-geo-leads/1.0");

            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Overpass returned HTTP {Status}: {Reason}", (int)response.StatusCode, response.ReasonPhrase);
                return Array.Empty<GeoLeadSourceRecordInput>();
            }

            var payload = await response.Content.ReadFromJsonAsync<OverpassResponse>(cancellationToken: cancellationToken);
            if (payload?.Elements is null) return Array.Empty<GeoLeadSourceRecordInput>();

            var records = new List<GeoLeadSourceRecordInput>(payload.Elements.Count);
            foreach (var el in payload.Elements)
            {
                if (el.Type != "node" || el.Tags is null) continue;
                var name = el.Tags.GetValueOrDefault("name") ?? el.Tags.GetValueOrDefault("name:en");
                if (string.IsNullOrWhiteSpace(name)) continue;

                var category = ClassifyCategory(el.Tags);
                if (category is null) continue;

                var address = BuildAddress(el.Tags);
                var phone = el.Tags.GetValueOrDefault("contact:phone") ?? el.Tags.GetValueOrDefault("phone");
                var email = el.Tags.GetValueOrDefault("contact:email") ?? el.Tags.GetValueOrDefault("email");
                var website = el.Tags.GetValueOrDefault("contact:website") ?? el.Tags.GetValueOrDefault("website");

                records.Add(new GeoLeadSourceRecordInput(
                    SourceRecordId: $"osm-node-{el.Id}",
                    RawName: name!,
                    RawCategory: category,
                    RawAddress: address,
                    RawPhone: phone,
                    RawEmail: email,
                    RawWebsite: website,
                    RawLatitude: (decimal?)el.Lat,
                    RawLongitude: (decimal?)el.Lon,
                    RawPayloadJson: JsonSerializer.Serialize(new
                    {
                        osmId = el.Id,
                        tags = el.Tags,
                        source = SourceName,
                    })));
            }

            logger.LogInformation("Overpass fetched {Count} POIs in bbox {Bbox}", records.Count, bbox);
            return records;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Overpass fetch failed for bbox {Bbox}", bbox);
            return Array.Empty<GeoLeadSourceRecordInput>();
        }
    }

    private static string? ClassifyCategory(IReadOnlyDictionary<string, string> tags)
    {
        if (tags.TryGetValue("tourism", out var t))
        {
            return t switch
            {
                "hotel" or "guest_house" or "hostel"   => "hotel",
                "attraction" or "museum" or "viewpoint" => "attraction",
                "information"                            => "transport",
                _                                        => null,
            };
        }
        if (tags.TryGetValue("shop", out var s) && s == "travel_agency") return "tour_operator";
        if (tags.TryGetValue("office", out var o) && o == "travel_agent") return "tour_operator";
        if (tags.TryGetValue("amenity", out var a) && a == "restaurant") return "restaurant";
        return null;
    }

    private static string? BuildAddress(IReadOnlyDictionary<string, string> tags)
    {
        var parts = new[]
        {
            tags.GetValueOrDefault("addr:housenumber"),
            tags.GetValueOrDefault("addr:street"),
            tags.GetValueOrDefault("addr:suburb"),
            tags.GetValueOrDefault("addr:city"),
            tags.GetValueOrDefault("addr:postcode"),
        }.Where(x => !string.IsNullOrWhiteSpace(x));
        var joined = string.Join(", ", parts);
        return string.IsNullOrWhiteSpace(joined) ? null : joined;
    }

    private sealed class OverpassResponse
    {
        [JsonPropertyName("elements")] public List<OverpassElement>? Elements { get; set; }
    }

    private sealed class OverpassElement
    {
        [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
        [JsonPropertyName("id")]   public long Id { get; set; }
        [JsonPropertyName("lat")]  public double Lat { get; set; }
        [JsonPropertyName("lon")]  public double Lon { get; set; }
        [JsonPropertyName("tags")] public Dictionary<string, string>? Tags { get; set; }
    }
}
