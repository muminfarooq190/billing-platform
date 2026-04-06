using Microsoft.EntityFrameworkCore;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class QuotationRevisionRepository(TravelDbContext dbContext) : IQuotationRevisionRepository
{
    public Task AddAsync(QuotationRevision revision, CancellationToken cancellationToken)
        => dbContext.QuotationRevisions.AddAsync(revision, cancellationToken).AsTask();

    public Task<QuotationRevision?> GetByIdAsync(Guid quotationId, Guid revisionId, CancellationToken cancellationToken)
        => dbContext.QuotationRevisions
            .Include(x => x.LineItems)
            .SingleOrDefaultAsync(x => x.QuotationId == quotationId && x.Id == revisionId, cancellationToken);

    public async Task<IReadOnlyList<QuotationRevision>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken)
        => await dbContext.QuotationRevisions
            .Where(x => x.QuotationId == quotationId)
            .OrderByDescending(x => x.RevisionNumber)
            .ToListAsync(cancellationToken);
}
