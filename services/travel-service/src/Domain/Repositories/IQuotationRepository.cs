using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface IQuotationRepository
{
    Task AddAsync(Quotation quotation, CancellationToken cancellationToken);
    Task<Quotation?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Quotation>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken);
    Task UpdateAsync(Quotation quotation, CancellationToken cancellationToken);
}
