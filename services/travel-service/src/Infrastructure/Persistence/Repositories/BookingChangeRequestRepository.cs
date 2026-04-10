using Microsoft.EntityFrameworkCore;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class BookingChangeRequestRepository(TravelDbContext dbContext) : IBookingChangeRequestRepository
{
    public Task AddAsync(BookingChangeRequest request, CancellationToken cancellationToken)
        => dbContext.AddAsync(request, cancellationToken).AsTask();

    public Task<BookingChangeRequest?> GetByIdAsync(Guid bookingId, Guid changeRequestId, CancellationToken cancellationToken)
        => dbContext.Set<BookingChangeRequest>().SingleOrDefaultAsync(x => x.BookingId == bookingId && x.Id == changeRequestId, cancellationToken);

    public async Task<IReadOnlyList<BookingChangeRequest>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken)
        => await dbContext.Set<BookingChangeRequest>()
            .Where(x => x.BookingId == bookingId)
            .OrderByDescending(x => x.RequestedAt)
            .ToListAsync(cancellationToken);

    public Task UpdateAsync(BookingChangeRequest request, CancellationToken cancellationToken)
    {
        dbContext.Update(request);
        return Task.CompletedTask;
    }
}
