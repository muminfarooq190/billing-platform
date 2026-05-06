using MediatR;

namespace TravelService.Application.Queries.TravelTemplates;

public sealed record ListTravelTemplatesQuery(Guid TenantId, string? Context) : IRequest<IReadOnlyList<TravelTemplateReadModel>>;
