using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;

namespace TravelService.Domain.Repositories;

public interface ITenantActiveTemplateRepository
{
    Task<TenantActiveTemplate?> GetAsync(Guid tenantId, TravelTemplateContext context, CancellationToken cancellationToken);
    Task UpsertAsync(TenantActiveTemplate activeTemplate, CancellationToken cancellationToken);
}
