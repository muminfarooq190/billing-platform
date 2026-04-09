using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface IBookingChangeRequestRepository
{
    Task AddAsync(BookingChangeRequest request, CancellationToken cancellationToken);
    Task<BookingChangeRequest?> GetByIdAsync(Guid bookingId, Guid changeRequestId, CancellationToken cancellationToken);
    Task<IReadOnlyList<BookingChangeRequest>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken);
    Task UpdateAsync(BookingChangeRequest request, CancellationToken cancellationToken);
}
