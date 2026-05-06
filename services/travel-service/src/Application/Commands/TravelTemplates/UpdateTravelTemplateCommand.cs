using MediatR;

namespace TravelService.Application.Commands.TravelTemplates;

public sealed record UpdateTravelTemplateCommand(
    Guid TenantId,
    Guid TemplateId,
    string Name,
    string? Description,
    string Category,
    string Banner,
    string AccentColor,
    string Tagline,
    List<TravelTemplateSectionCommandModel> Sections,
    TravelTemplateSeedCommandModel Seed) : IRequest;
