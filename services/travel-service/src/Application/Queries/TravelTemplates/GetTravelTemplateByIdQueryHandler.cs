using MediatR;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Queries.TravelTemplates;

public sealed class GetTravelTemplateByIdQueryHandler(ITravelTemplateRepository templateRepository) : IRequestHandler<GetTravelTemplateByIdQuery, TravelTemplateReadModel?>
{
    public async Task<TravelTemplateReadModel?> Handle(GetTravelTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null || template.TenantId != request.TenantId)
            return null;

        return TravelTemplateQuerySupport.ToReadModel(template);
    }
}
