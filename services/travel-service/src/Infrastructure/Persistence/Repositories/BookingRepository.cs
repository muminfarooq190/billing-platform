using Microsoft.EntityFrameworkCore;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class BookingRepository(TravelDbContext dbContext) : IBookingRepository
{
    public Task AddAsync(Booking booking, CancellationToken cancellationToken)
        => dbContext.Bookings.AddAsync(booking, cancellationToken).AsTask();

    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Bookings.SingleOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

    public Task<Booking?> GetByAcceptedRevisionIdAsync(Guid acceptedRevisionId, CancellationToken cancellationToken)
        => dbContext.Bookings.SingleOrDefaultAsync(x => x.AcceptedRevisionId == acceptedRevisionId && x.DeletedAt == null, cancellationToken);

    public async Task<IReadOnlyList<Booking>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken)
        => await dbContext.Bookings.Where(x => x.TenantId == tenantId && x.DeletedAt == null).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

    public Task UpdateAsync(Booking booking, CancellationToken cancellationToken)
    {
        dbContext.Bookings.Update(booking);
        return Task.CompletedTask;
    }
}
