using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface ITravelInquiryStatusHistoryRepository
{
    Task AddAsync(TravelInquiryStatusHistory entry, CancellationToken cancellationToken);
}
