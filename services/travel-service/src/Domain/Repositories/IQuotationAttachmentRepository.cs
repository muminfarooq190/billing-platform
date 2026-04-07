using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface IQuotationAttachmentRepository
{
    Task AddAsync(QuotationAttachment attachment, CancellationToken cancellationToken);
    Task<QuotationAttachment?> GetByIdAsync(Guid quotationId, Guid attachmentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<QuotationAttachment>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken);
    Task UpdateAsync(QuotationAttachment attachment, CancellationToken cancellationToken);
}
