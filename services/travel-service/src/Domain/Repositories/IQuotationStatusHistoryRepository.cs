using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface IQuotationStatusHistoryRepository
{
    Task AddAsync(QuotationStatusHistory entry, CancellationToken cancellationToken);
}
