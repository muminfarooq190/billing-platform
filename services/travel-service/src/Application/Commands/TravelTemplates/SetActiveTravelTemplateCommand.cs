using MediatR;

namespace TravelService.Application.Commands.TravelTemplates;

public sealed record SetActiveTravelTemplateCommand(Guid TenantId, string Context, Guid? TemplateId) : IRequest;
