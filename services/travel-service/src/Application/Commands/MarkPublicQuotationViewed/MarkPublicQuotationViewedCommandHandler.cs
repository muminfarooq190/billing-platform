using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.MarkPublicQuotationViewed;

public sealed class MarkPublicQuotationViewedCommandHandler(
    IQuotationShareLinkRepository quotationShareLinkRepository,
    IQuotationRepository quotationRepository,
    IQuotationStatusHistoryRepository quotationStatusHistoryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<MarkPublicQuotationViewedCommand, bool>
{
    public async Task<bool> Handle(MarkPublicQuotationViewedCommand request, CancellationToken cancellationToken)
    {
        var shareLink = await quotationShareLinkRepository.GetActiveByTokenAsync(request.Token, cancellationToken);
        if (shareLink is null || !shareLink.IsActive(DateTimeOffset.UtcNow))
            return false;

        var quotation = await quotationRepository.GetByIdAsync(shareLink.QuotationId, cancellationToken);
        if (quotation is null)
            return false;

        var previousStatus = quotation.Status.ToString();
        shareLink.MarkViewed();
        quotation.MarkViewed(shareLink.LastViewedAt);

        if (quotation.Status == Domain.Enums.QuotationStatus.Sent)
        {
            await quotationStatusHistoryRepository.AddAsync(
                QuotationStatusHistory.Create(quotation.Id, quotation.TenantId, previousStatus, "Viewed", "Public quotation viewed by customer."),
                cancellationToken);
        }

        await quotationShareLinkRepository.UpdateAsync(shareLink, cancellationToken);
        await quotationRepository.UpdateAsync(quotation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
