using Microsoft.EntityFrameworkCore;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class QuotationAttachmentRepository(TravelDbContext dbContext) : IQuotationAttachmentRepository
{
    public Task AddAsync(QuotationAttachment attachment, CancellationToken cancellationToken)
        => dbContext.QuotationAttachments.AddAsync(attachment, cancellationToken).AsTask();

    public Task<QuotationAttachment?> GetByIdAsync(Guid quotationId, Guid attachmentId, CancellationToken cancellationToken)
        => dbContext.QuotationAttachments.SingleOrDefaultAsync(
            x => x.QuotationId == quotationId && x.Id == attachmentId && x.DeletedAt == null,
            cancellationToken);

    public async Task<IReadOnlyList<QuotationAttachment>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken)
        => await dbContext.QuotationAttachments
            .Where(x => x.QuotationId == quotationId && x.DeletedAt == null)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task UpdateAsync(QuotationAttachment attachment, CancellationToken cancellationToken)
    {
        dbContext.QuotationAttachments.Update(attachment);
        return Task.CompletedTask;
    }
}
