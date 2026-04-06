using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface IQuotationRevisionRepository
{
    Task AddAsync(QuotationRevision revision, CancellationToken cancellationToken);
    Task<QuotationRevision?> GetByIdAsync(Guid quotationId, Guid revisionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<QuotationRevision>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken);
}
