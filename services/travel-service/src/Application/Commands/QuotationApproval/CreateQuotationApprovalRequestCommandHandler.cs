using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.QuotationApproval;

public sealed class CreateQuotationApprovalRequestCommandHandler(
    IQuotationRepository quotationRepository,
    IQuotationRevisionRepository quotationRevisionRepository,
    IQuotationApprovalRequestRepository approvalRequestRepository,
    IActivityWriter activityWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateQuotationApprovalRequestCommand, Guid>
{
    public async Task<Guid> Handle(CreateQuotationApprovalRequestCommand request, CancellationToken cancellationToken)
    {
        var quotation = await quotationRepository.GetByIdAsync(request.QuotationId, cancellationToken)
            ?? throw new DomainException($"Quotation {request.QuotationId} not found.");
        if (quotation.TenantId != request.TenantId)
            throw new DomainException("Quotation does not belong to the active tenant.");

        var revision = await quotationRevisionRepository.GetByIdAsync(request.QuotationId, request.RevisionId, cancellationToken)
            ?? throw new DomainException("Quotation revision not found.");
        if (revision.TenantId != request.TenantId)
            throw new DomainException("Quotation revision does not belong to the active tenant.");

        var approvalRequest = QuotationApprovalRequest.Create(
            quotation.Id,
            quotation.TenantId,
            revision.Id,
            request.Reason,
            revision.TotalAmount,
            request.MarginPercent,
            request.DiscountPercent,
            actorContext.UserId);

        await approvalRequestRepository.AddAsync(approvalRequest, cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(quotation.TenantId, "Quotation", quotation.Id, "ApprovalRequested", "Quotation approval requested", new
            {
                approvalRequest.Id,
                approvalRequest.RevisionId,
                approvalRequest.TotalAmount,
                approvalRequest.MarginPercent,
                approvalRequest.DiscountPercent
            }, actorContext.UserId),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return approvalRequest.Id;
    }
}
