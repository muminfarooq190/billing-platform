using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface IQuotationApprovalRequestRepository
{
    Task AddAsync(QuotationApprovalRequest request, CancellationToken cancellationToken);
    Task<QuotationApprovalRequest?> GetByIdAsync(Guid quotationId, Guid approvalRequestId, CancellationToken cancellationToken);
    Task<IReadOnlyList<QuotationApprovalRequest>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken);
    Task UpdateAsync(QuotationApprovalRequest request, CancellationToken cancellationToken);
}
