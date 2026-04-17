using Microsoft.EntityFrameworkCore;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class TravelInquiryRepository(TravelDbContext dbContext) : ITravelInquiryRepository
{
    public Task AddAsync(TravelInquiry inquiry, CancellationToken cancellationToken)
        => dbContext.TravelInquiries.AddAsync(inquiry, cancellationToken).AsTask();

    public Task<TravelInquiry?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.TravelInquiries.SingleOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

    public Task UpdateAsync(TravelInquiry inquiry, CancellationToken cancellationToken)
    {
        dbContext.TravelInquiries.Update(inquiry);
        return Task.CompletedTask;
    }
}
