using Microsoft.EntityFrameworkCore;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class TenantActiveTemplateRepository(TravelDbContext dbContext) : ITenantActiveTemplateRepository
{
    public Task<TenantActiveTemplate?> GetAsync(Guid tenantId, TravelTemplateContext context, CancellationToken cancellationToken)
        => dbContext.Set<TenantActiveTemplate>().SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Context == context, cancellationToken);

    public Task UpsertAsync(TenantActiveTemplate activeTemplate, CancellationToken cancellationToken)
    {
        dbContext.Set<TenantActiveTemplate>().Update(activeTemplate);
        return Task.CompletedTask;
    }
}
