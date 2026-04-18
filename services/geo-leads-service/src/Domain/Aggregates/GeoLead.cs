namespace GeoLeadsService.Domain.Aggregates;

public sealed record GeoLead(
    Guid Id,
    string CanonicalName,
    string LeadType,
    string? PrimaryEmail,
    string? PrimaryPhone,
    string? Website,
    string Address,
    decimal Latitude,
    decimal Longitude,
    string City,
    string Region,
    string Country,
    decimal ConfidenceScore,
    decimal ContactabilityScore,
    decimal TourismRelevanceScore,
    IReadOnlyList<string> Sources,
    IReadOnlyList<string> Reasons,
    DateTimeOffset UpdatedAt);
