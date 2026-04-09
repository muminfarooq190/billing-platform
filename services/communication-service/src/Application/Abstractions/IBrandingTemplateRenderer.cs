namespace CommunicationService.Application.Abstractions;

public interface IBrandingTemplateRenderer
{
    Task<Dictionary<string, string>> EnrichAsync(Guid tenantId, string scope, Dictionary<string, string> placeholders, CancellationToken cancellationToken);
}
