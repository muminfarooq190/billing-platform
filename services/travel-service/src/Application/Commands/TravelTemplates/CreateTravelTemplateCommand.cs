using MediatR;

namespace TravelService.Application.Commands.TravelTemplates;

public sealed record CreateTravelTemplateCommand(
    Guid TenantId,
    string Context,
    string Name,
    string? Description,
    string Category,
    string Banner,
    string AccentColor,
    string Tagline,
    List<TravelTemplateSectionCommandModel> Sections,
    TravelTemplateSeedCommandModel Seed,
    Guid? CreatedByUserId) : IRequest<Guid>;
