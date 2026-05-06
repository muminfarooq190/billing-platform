using Microsoft.EntityFrameworkCore;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class BookingPaymentRepository(TravelDbContext dbContext) : IBookingPaymentRepository
{
    public Task AddAsync(BookingPayment payment, CancellationToken cancellationToken)
        => dbContext.Set<BookingPayment>().AddAsync(payment, cancellationToken).AsTask();

    public Task<BookingPayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Set<BookingPayment>().SingleOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

    public async Task<IReadOnlyList<BookingPayment>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken)
        => await dbContext.Set<BookingPayment>()
            .Where(x => x.BookingId == bookingId && x.DeletedAt == null)
            .OrderBy(x => x.DueDate)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task UpdateAsync(BookingPayment payment, CancellationToken cancellationToken)
    {
        dbContext.Set<BookingPayment>().Update(payment);
        return Task.CompletedTask;
    }
}
