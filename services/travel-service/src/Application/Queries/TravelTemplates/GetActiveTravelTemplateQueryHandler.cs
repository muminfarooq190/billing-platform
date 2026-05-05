using MediatR;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Queries.TravelTemplates;

public sealed class GetActiveTravelTemplateQueryHandler(ITenantActiveTemplateRepository activeTemplateRepository) : IRequestHandler<GetActiveTravelTemplateQuery, ActiveTravelTemplateReadModel>
{
    public async Task<ActiveTravelTemplateReadModel> Handle(GetActiveTravelTemplateQuery request, CancellationToken cancellationToken)
    {
        var context = TravelTemplateQuerySupport.ParseContext(request.Context);
        var active = await activeTemplateRepository.GetAsync(request.TenantId, context, cancellationToken);
        return new ActiveTravelTemplateReadModel(context.ToString(), active?.TemplateId, active?.UpdatedAt);
    }
}
