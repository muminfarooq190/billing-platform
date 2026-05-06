namespace TravelService.Application.Commands.TravelTemplates;

public sealed record TravelTemplateSectionCommandModel(string Id, string Label, string Hint);
public sealed record TravelTemplateConceptSeedCommandModel(string Type, string Content);
public sealed record TravelTemplateQuoteSeedCommandModel(string Type, string Title, string Description, decimal Amount);
public sealed record TravelTemplateItineraryItemSeedCommandModel(string Type, string Title, string? Time, string? Notes);
public sealed record TravelTemplateItineraryDaySeedCommandModel(string Title, List<TravelTemplateItineraryItemSeedCommandModel> Items);
public sealed record TravelTemplateSeedCommandModel(
    List<TravelTemplateConceptSeedCommandModel> ConceptSeed,
    List<TravelTemplateQuoteSeedCommandModel> QuoteSeed,
    List<TravelTemplateItineraryDaySeedCommandModel> ItineraryDays);
