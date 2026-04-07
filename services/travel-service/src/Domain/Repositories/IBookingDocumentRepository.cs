using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface IBookingDocumentRepository
{
    Task AddAsync(BookingDocument document, CancellationToken cancellationToken);
    Task<BookingDocument?> GetByIdAsync(Guid bookingId, Guid documentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<BookingDocument>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken);
    Task UpdateAsync(BookingDocument document, CancellationToken cancellationToken);
}
