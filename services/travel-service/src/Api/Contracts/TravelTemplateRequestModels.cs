namespace TravelService.Api.Contracts;

public sealed record TravelTemplateSectionRequest(string Id, string Label, string Hint);
public sealed record TravelTemplateConceptSeedRequest(string Type, string Content);
public sealed record TravelTemplateQuoteSeedRequest(string Type, string Title, string Description, decimal Amount);
public sealed record TravelTemplateItineraryItemSeedRequest(string Type, string Title, string? Time, string? Notes);
public sealed record TravelTemplateItineraryDaySeedRequest(string Title, List<TravelTemplateItineraryItemSeedRequest> Items);
public sealed record TravelTemplateSeedRequest(
    List<TravelTemplateConceptSeedRequest> ConceptSeed,
    List<TravelTemplateQuoteSeedRequest> QuoteSeed,
    List<TravelTemplateItineraryDaySeedRequest> ItineraryDays);

public sealed record CreateTravelTemplateRequest(
    string Context,
    string Name,
    string? Description,
    string Category,
    string Banner,
    string AccentColor,
    string Tagline,
    List<TravelTemplateSectionRequest> Sections,
    TravelTemplateSeedRequest Seed);

public sealed record UpdateTravelTemplateRequest(
    string Name,
    string? Description,
    string Category,
    string Banner,
    string AccentColor,
    string Tagline,
    List<TravelTemplateSectionRequest> Sections,
    TravelTemplateSeedRequest Seed);

public sealed record SetActiveTravelTemplateRequest(string Context, Guid? TemplateId);
