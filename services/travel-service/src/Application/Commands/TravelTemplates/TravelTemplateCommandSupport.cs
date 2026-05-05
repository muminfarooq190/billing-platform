using System.Text.Json;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.TravelTemplates;

internal static class TravelTemplateCommandSupport
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<TravelTemplate> LoadTemplateAsync(ITravelTemplateRepository templateRepository, Guid tenantId, Guid templateId, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(templateId, cancellationToken)
            ?? throw new DomainException($"Travel template {templateId} not found.");

        if (template.TenantId != tenantId)
            throw new DomainException("Travel template does not belong to tenant context.");

        return template;
    }

    public static string SerializeSections(IReadOnlyCollection<TravelTemplateSectionCommandModel> sections)
    {
        if (sections.Count == 0)
            throw new DomainException("At least one template section is required.");

        return JsonSerializer.Serialize(new { sections }, JsonOptions);
    }

    public static string SerializeSeed(TravelTemplateSeedCommandModel seed)
        => JsonSerializer.Serialize(seed, JsonOptions);
}
