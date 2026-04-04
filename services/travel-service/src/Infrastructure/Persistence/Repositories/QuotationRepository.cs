using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class QuotationRepository(TravelDbContext dbContext) : IQuotationRepository
{
    public Task AddAsync(Quotation quotation, CancellationToken cancellationToken) => dbContext.Quotations.AddAsync(quotation, cancellationToken).AsTask();

    public Task<Quotation?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => dbContext.Quotations.SingleOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

    public async Task<IReadOnlyList<Quotation>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => await dbContext.Quotations.Where(x => x.TenantId == tenantId && x.DeletedAt == null).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

    public Task UpdateAsync(Quotation quotation, CancellationToken cancellationToken)
    {
        dbContext.Quotations.Update(quotation);
        return Task.CompletedTask;
    }
}
