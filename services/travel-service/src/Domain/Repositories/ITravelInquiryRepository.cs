using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface ITravelInquiryRepository
{
    Task AddAsync(TravelInquiry inquiry, CancellationToken cancellationToken);
    Task<TravelInquiry?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task UpdateAsync(TravelInquiry inquiry, CancellationToken cancellationToken);
}
