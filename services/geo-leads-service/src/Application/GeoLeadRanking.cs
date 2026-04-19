using GeoLeadsService.Domain.Aggregates;

namespace GeoLeadsService.Application;

public static class GeoLeadRanking
{
    public static decimal Score(GeoLead lead, string? rankingMode)
    {
        var mode = NormalizeMode(rankingMode);

        return mode switch
        {
            "contactability" => Math.Round((lead.ContactabilityScore * 0.50m) + (lead.ConfidenceScore * 0.30m) + (lead.TourismRelevanceScore * 0.20m), 4),
            "popularity" => Math.Round((lead.TourismRelevanceScore * 0.55m) + (lead.ConfidenceScore * 0.25m) + (lead.ContactabilityScore * 0.20m), 4),
            _ => Math.Round((lead.TourismRelevanceScore * 0.35m) + (lead.ContactabilityScore * 0.30m) + (lead.ConfidenceScore * 0.35m), 4)
        };
    }

    public static string NormalizeMode(string? rankingMode)
        => string.IsNullOrWhiteSpace(rankingMode)
            ? "relevance"
            : rankingMode.Trim().ToLowerInvariant() switch
            {
                "relevance" => "relevance",
                "contactability" => "contactability",
                "popularity" => "popularity",
                _ => "relevance"
            };
}
