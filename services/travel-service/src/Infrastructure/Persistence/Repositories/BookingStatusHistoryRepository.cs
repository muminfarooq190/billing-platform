using Microsoft.EntityFrameworkCore;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class BookingStatusHistoryRepository(TravelDbContext dbContext) : IBookingStatusHistoryRepository
{
    public Task AddAsync(BookingStatusHistory history, CancellationToken cancellationToken)
        => dbContext.BookingStatusHistory.AddAsync(history, cancellationToken).AsTask();

    public async Task<IReadOnlyList<BookingStatusHistory>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken)
        => await dbContext.BookingStatusHistory.Where(x => x.BookingId == bookingId).OrderBy(x => x.CreatedAt).ToListAsync(cancellationToken);
}
