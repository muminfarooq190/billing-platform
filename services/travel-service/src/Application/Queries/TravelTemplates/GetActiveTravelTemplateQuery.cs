using MediatR;

namespace TravelService.Application.Queries.TravelTemplates;

public sealed record GetActiveTravelTemplateQuery(Guid TenantId, string Context) : IRequest<ActiveTravelTemplateReadModel>;
