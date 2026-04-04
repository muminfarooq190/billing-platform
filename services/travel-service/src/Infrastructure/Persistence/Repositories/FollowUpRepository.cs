using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class FollowUpRepository(TravelDbContext dbContext) : IFollowUpRepository
{
    public Task AddAsync(FollowUp followUp, CancellationToken cancellationToken) => dbContext.FollowUps.AddAsync(followUp, cancellationToken).AsTask();

    public Task<FollowUp?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => dbContext.FollowUps.SingleOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

    public async Task<IReadOnlyList<FollowUp>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => await dbContext.FollowUps.Where(x => x.TenantId == tenantId && x.DeletedAt == null).OrderBy(x => x.DueDate).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<FollowUp>> ListOverdueAsync(DateTimeOffset asOf, CancellationToken cancellationToken) => await dbContext.FollowUps.Where(x => x.DueDate <= asOf && x.Status != Domain.Enums.FollowUpStatus.Completed && x.Status != Domain.Enums.FollowUpStatus.Cancelled && x.DeletedAt == null).ToListAsync(cancellationToken);

    public Task UpdateAsync(FollowUp followUp, CancellationToken cancellationToken)
    {
        dbContext.FollowUps.Update(followUp);
        return Task.CompletedTask;
    }
}
