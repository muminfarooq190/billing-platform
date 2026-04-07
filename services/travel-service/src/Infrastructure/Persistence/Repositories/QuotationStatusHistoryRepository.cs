using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class QuotationStatusHistoryRepository(TravelDbContext dbContext) : IQuotationStatusHistoryRepository
{
    public Task AddAsync(QuotationStatusHistory entry, CancellationToken cancellationToken)
        => dbContext.QuotationStatusHistory.AddAsync(entry, cancellationToken).AsTask();
}
