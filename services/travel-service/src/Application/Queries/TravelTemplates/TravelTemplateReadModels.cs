namespace TravelService.Application.Queries.TravelTemplates;

public sealed record TravelTemplateSectionReadModel(string Id, string Label, string Hint);
public sealed record TravelTemplateConceptSeedReadModel(string Type, string Content);
public sealed record TravelTemplateQuoteSeedReadModel(string Type, string Title, string Description, decimal Amount);
public sealed record TravelTemplateItineraryItemSeedReadModel(string Type, string Title, string? Time, string? Notes);
public sealed record TravelTemplateItineraryDaySeedReadModel(string Title, IReadOnlyList<TravelTemplateItineraryItemSeedReadModel> Items);
public sealed record TravelTemplateSeedReadModel(
    IReadOnlyList<TravelTemplateConceptSeedReadModel> ConceptSeed,
    IReadOnlyList<TravelTemplateQuoteSeedReadModel> QuoteSeed,
    IReadOnlyList<TravelTemplateItineraryDaySeedReadModel> ItineraryDays);

public sealed record TravelTemplateReadModel(
    Guid Id,
    string Context,
    string Name,
    string? Description,
    string Category,
    string Banner,
    string AccentColor,
    string Tagline,
    IReadOnlyList<TravelTemplateSectionReadModel> Sections,
    TravelTemplateSeedReadModel Seed,
    bool IsBuiltIn,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ActiveTravelTemplateReadModel(string Context, Guid? TemplateId, DateTimeOffset? UpdatedAt);
