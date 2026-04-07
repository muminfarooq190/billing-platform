using Microsoft.EntityFrameworkCore;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class BookingDocumentRepository(TravelDbContext dbContext) : IBookingDocumentRepository
{
    public Task AddAsync(BookingDocument document, CancellationToken cancellationToken)
        => dbContext.BookingDocuments.AddAsync(document, cancellationToken).AsTask();

    public Task<BookingDocument?> GetByIdAsync(Guid bookingId, Guid documentId, CancellationToken cancellationToken)
        => dbContext.BookingDocuments.SingleOrDefaultAsync(x => x.BookingId == bookingId && x.Id == documentId && x.DeletedAt == null, cancellationToken);

    public async Task<IReadOnlyList<BookingDocument>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken)
        => await dbContext.BookingDocuments.Where(x => x.BookingId == bookingId && x.DeletedAt == null).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

    public Task UpdateAsync(BookingDocument document, CancellationToken cancellationToken)
    {
        dbContext.BookingDocuments.Update(document);
        return Task.CompletedTask;
    }
}
