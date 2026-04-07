using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface IQuotationShareLinkRepository
{
    Task AddAsync(QuotationShareLink shareLink, CancellationToken cancellationToken);
    Task<QuotationShareLink?> GetActiveByTokenAsync(string token, CancellationToken cancellationToken);
    Task<IReadOnlyList<QuotationShareLink>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken);
    Task UpdateAsync(QuotationShareLink shareLink, CancellationToken cancellationToken);
}
