using Microsoft.EntityFrameworkCore;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class QuotationApprovalRequestRepository(TravelDbContext dbContext) : IQuotationApprovalRequestRepository
{
    public Task AddAsync(QuotationApprovalRequest request, CancellationToken cancellationToken)
        => dbContext.AddAsync(request, cancellationToken).AsTask();

    public Task<QuotationApprovalRequest?> GetByIdAsync(Guid quotationId, Guid approvalRequestId, CancellationToken cancellationToken)
        => dbContext.Set<QuotationApprovalRequest>().SingleOrDefaultAsync(x => x.QuotationId == quotationId && x.Id == approvalRequestId, cancellationToken);

    public async Task<IReadOnlyList<QuotationApprovalRequest>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken)
        => await dbContext.Set<QuotationApprovalRequest>()
            .Where(x => x.QuotationId == quotationId)
            .OrderByDescending(x => x.RequestedAt)
            .ToListAsync(cancellationToken);

    public Task UpdateAsync(QuotationApprovalRequest request, CancellationToken cancellationToken)
    {
        dbContext.Update(request);
        return Task.CompletedTask;
    }
}
