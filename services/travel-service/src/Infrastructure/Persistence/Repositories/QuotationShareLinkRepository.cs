using Microsoft.EntityFrameworkCore;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class QuotationShareLinkRepository(TravelDbContext dbContext) : IQuotationShareLinkRepository
{
    public Task AddAsync(QuotationShareLink shareLink, CancellationToken cancellationToken)
        => dbContext.QuotationShareLinks.AddAsync(shareLink, cancellationToken).AsTask();

    public Task<QuotationShareLink?> GetActiveByTokenAsync(string token, CancellationToken cancellationToken)
        => dbContext.QuotationShareLinks
            .SingleOrDefaultAsync(x => x.Token == token && x.RevokedAt == null, cancellationToken);

    public async Task<IReadOnlyList<QuotationShareLink>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken)
        => await dbContext.QuotationShareLinks
            .Where(x => x.QuotationId == quotationId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task UpdateAsync(QuotationShareLink shareLink, CancellationToken cancellationToken)
    {
        dbContext.QuotationShareLinks.Update(shareLink);
        return Task.CompletedTask;
    }
}
