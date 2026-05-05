namespace TravelService.Application.Queries.TravelTemplates;

internal sealed class TravelTemplateSectionsEnvelope
{
    public List<TravelTemplateSectionReadModel> Sections { get; set; } = [];
}

internal sealed class TravelTemplateSeedEnvelope
{
    public List<TravelTemplateConceptSeedReadModel> ConceptSeed { get; set; } = [];
    public List<TravelTemplateQuoteSeedReadModel> QuoteSeed { get; set; } = [];
    public List<TravelTemplateItineraryDaySeedReadModel> ItineraryDays { get; set; } = [];
}
