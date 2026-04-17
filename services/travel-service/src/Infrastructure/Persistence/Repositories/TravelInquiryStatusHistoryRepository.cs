using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class TravelInquiryStatusHistoryRepository(TravelDbContext dbContext) : ITravelInquiryStatusHistoryRepository
{
    public Task AddAsync(TravelInquiryStatusHistory entry, CancellationToken cancellationToken)
        => dbContext.TravelInquiryStatusHistory.AddAsync(entry, cancellationToken).AsTask();
}
