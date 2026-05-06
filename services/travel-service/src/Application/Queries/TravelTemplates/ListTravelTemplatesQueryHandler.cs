using MediatR;
using TravelService.Domain.Enums;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Queries.TravelTemplates;

public sealed class ListTravelTemplatesQueryHandler(ITravelTemplateRepository templateRepository) : IRequestHandler<ListTravelTemplatesQuery, IReadOnlyList<TravelTemplateReadModel>>
{
    public async Task<IReadOnlyList<TravelTemplateReadModel>> Handle(ListTravelTemplatesQuery request, CancellationToken cancellationToken)
    {
        TravelTemplateContext? context = null;
        if (!string.IsNullOrWhiteSpace(request.Context))
            context = TravelTemplateQuerySupport.ParseContext(request.Context);

        var templates = await templateRepository.ListByTenantAsync(request.TenantId, context, cancellationToken);
        return templates.Select(TravelTemplateQuerySupport.ToReadModel).ToList();
    }
}
