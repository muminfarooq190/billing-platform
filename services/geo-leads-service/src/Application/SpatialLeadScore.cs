using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;

namespace GeoLeadsService.Application;

public sealed record SpatialLeadScore(GeoLead Lead, decimal Score, decimal DistanceMeters, decimal IntersectionBoost);

public static class SpatialLeadScoring
{
    public static IReadOnlyList<SpatialLeadScore> Rank(GeoPolygon queryPolygon, IReadOnlyList<GeoLead> leads, string? rankingMode)
    {
        var centroid = GetCentroid(queryPolygon);

        return leads
            .Select(lead =>
            {
                var distanceMeters = DistanceMeters(centroid.Latitude, centroid.Longitude, lead.Latitude, lead.Longitude);
                var distanceBoost = Math.Max(0m, 1m - (distanceMeters / 10000m));
                var intersectionBoost = 0.15m + (distanceBoost * 0.20m);
                var baseScore = GeoLeadRanking.Score(lead, rankingMode);
                var finalScore = Math.Round(baseScore + intersectionBoost, 4);
                return new SpatialLeadScore(lead, finalScore, Math.Round(distanceMeters, 2), Math.Round(intersectionBoost, 4));
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.DistanceMeters)
            .ThenBy(x => x.Lead.CanonicalName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static GeoCoordinate GetCentroid(GeoPolygon polygon)
    {
        var coords = polygon.Coordinates;
        var avgLng = coords.Average(x => x.Longitude);
        var avgLat = coords.Average(x => x.Latitude);
        return new GeoCoordinate(avgLng, avgLat);
    }

    private static decimal DistanceMeters(decimal lat1, decimal lng1, decimal lat2, decimal lng2)
    {
        var r = 6371000d;
        var dLat = DegreesToRadians((double)(lat2 - lat1));
        var dLng = DegreesToRadians((double)(lng2 - lng1));
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians((double)lat1)) * Math.Cos(DegreesToRadians((double)lat2)) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return (decimal)(r * c);
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180d;
}
