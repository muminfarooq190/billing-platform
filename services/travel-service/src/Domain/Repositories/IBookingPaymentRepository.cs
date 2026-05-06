using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface IBookingPaymentRepository
{
    Task AddAsync(BookingPayment payment, CancellationToken cancellationToken);
    Task<BookingPayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<BookingPayment>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken);
    Task UpdateAsync(BookingPayment payment, CancellationToken cancellationToken);
}
