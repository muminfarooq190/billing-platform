namespace CommunicationService.Application.Abstractions;

public interface IBrandingTemplateRenderer
{
    Dictionary<string, string> Enrich(Guid tenantId, Dictionary<string, string> placeholders);
}
