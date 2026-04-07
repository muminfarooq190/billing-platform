using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface IBookingStatusHistoryRepository
{
    Task AddAsync(BookingStatusHistory history, CancellationToken cancellationToken);
    Task<IReadOnlyList<BookingStatusHistory>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken);
}
