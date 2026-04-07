using Microsoft.EntityFrameworkCore;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class BookingItemRepository(TravelDbContext dbContext) : IBookingItemRepository
{
    public Task AddAsync(BookingItem item, CancellationToken cancellationToken)
        => dbContext.BookingItems.AddAsync(item, cancellationToken).AsTask();

    public Task<BookingItem?> GetByIdAsync(Guid bookingId, Guid itemId, CancellationToken cancellationToken)
        => dbContext.BookingItems.SingleOrDefaultAsync(x => x.BookingId == bookingId && x.Id == itemId && x.DeletedAt == null, cancellationToken);

    public async Task<IReadOnlyList<BookingItem>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken)
        => await dbContext.BookingItems.Where(x => x.BookingId == bookingId && x.DeletedAt == null).OrderBy(x => x.SortOrder).ThenBy(x => x.CreatedAt).ToListAsync(cancellationToken);

    public Task UpdateAsync(BookingItem item, CancellationToken cancellationToken)
    {
        dbContext.BookingItems.Update(item);
        return Task.CompletedTask;
    }
}
