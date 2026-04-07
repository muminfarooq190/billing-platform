using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface IBookingItemRepository
{
    Task AddAsync(BookingItem item, CancellationToken cancellationToken);
    Task<BookingItem?> GetByIdAsync(Guid bookingId, Guid itemId, CancellationToken cancellationToken);
    Task<IReadOnlyList<BookingItem>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken);
    Task UpdateAsync(BookingItem item, CancellationToken cancellationToken);
}
