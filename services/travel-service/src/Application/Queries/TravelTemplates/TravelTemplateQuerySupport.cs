using System.Text.Json;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;

namespace TravelService.Application.Queries.TravelTemplates;

internal static class TravelTemplateQuerySupport
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static TravelTemplateReadModel ToReadModel(TravelTemplate template)
    {
        var sectionsEnvelope = JsonSerializer.Deserialize<TravelTemplateSectionsEnvelope>(template.SectionsJson, JsonOptions) ?? new TravelTemplateSectionsEnvelope();
        var seedEnvelope = JsonSerializer.Deserialize<TravelTemplateSeedEnvelope>(template.SeedJson, JsonOptions) ?? new TravelTemplateSeedEnvelope();

        return new TravelTemplateReadModel(
            template.Id,
            template.Context.ToString(),
            template.Name,
            template.Description,
            template.Category,
            template.Banner,
            template.AccentColor,
            template.Tagline,
            sectionsEnvelope.Sections,
            new TravelTemplateSeedReadModel(seedEnvelope.ConceptSeed, seedEnvelope.QuoteSeed, seedEnvelope.ItineraryDays),
            template.IsBuiltIn,
            template.IsActive,
            template.CreatedAt,
            template.UpdatedAt);
    }

    public static TravelTemplateContext ParseContext(string context)
        => Enum.TryParse<TravelTemplateContext>(context, true, out var parsed)
            ? parsed
            : throw new ArgumentException($"Unsupported template context '{context}'.", nameof(context));
}
