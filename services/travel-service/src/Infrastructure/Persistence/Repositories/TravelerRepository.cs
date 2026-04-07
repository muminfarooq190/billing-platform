using Microsoft.EntityFrameworkCore;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class TravelerRepository(TravelDbContext dbContext) : ITravelerRepository
{
    public Task AddAsync(Traveler traveler, CancellationToken cancellationToken)
        => dbContext.Travelers.AddAsync(traveler, cancellationToken).AsTask();

    public Task<Traveler?> GetByIdAsync(Guid bookingId, Guid travelerId, CancellationToken cancellationToken)
        => dbContext.Travelers.SingleOrDefaultAsync(x => x.BookingId == bookingId && x.Id == travelerId && x.DeletedAt == null, cancellationToken);

    public async Task<IReadOnlyList<Traveler>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken)
        => await dbContext.Travelers.Where(x => x.BookingId == bookingId && x.DeletedAt == null).OrderBy(x => x.CreatedAt).ToListAsync(cancellationToken);

    public Task UpdateAsync(Traveler traveler, CancellationToken cancellationToken)
    {
        dbContext.Travelers.Update(traveler);
        return Task.CompletedTask;
    }
}
