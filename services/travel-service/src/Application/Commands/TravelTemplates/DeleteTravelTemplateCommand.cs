using MediatR;

namespace TravelService.Application.Commands.TravelTemplates;

public sealed record DeleteTravelTemplateCommand(Guid TenantId, Guid TemplateId) : IRequest;
