using MediatR;

namespace TravelService.Application.Queries.TravelTemplates;

public sealed record GetTravelTemplateByIdQuery(Guid TenantId, Guid TemplateId) : IRequest<TravelTemplateReadModel?>;
