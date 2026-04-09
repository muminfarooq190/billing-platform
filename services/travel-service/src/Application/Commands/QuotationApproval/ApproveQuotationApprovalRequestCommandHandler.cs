using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.QuotationApproval;

public sealed class ApproveQuotationApprovalRequestCommandHandler(
    IQuotationRepository quotationRepository,
    IQuotationApprovalRequestRepository approvalRequestRepository,
    IActivityWriter activityWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<ApproveQuotationApprovalRequestCommand>
{
    public async Task Handle(ApproveQuotationApprovalRequestCommand request, CancellationToken cancellationToken)
    {
        var quotation = await quotationRepository.GetByIdAsync(request.QuotationId, cancellationToken)
            ?? throw new DomainException($"Quotation {request.QuotationId} not found.");
        if (quotation.TenantId != request.TenantId)
            throw new DomainException("Quotation does not belong to the active tenant.");

        var approvalRequest = await approvalRequestRepository.GetByIdAsync(request.QuotationId, request.ApprovalRequestId, cancellationToken)
            ?? throw new DomainException("Quotation approval request not found.");
        if (approvalRequest.TenantId != request.TenantId)
            throw new DomainException("Quotation approval request does not belong to the active tenant.");

        approvalRequest.Approve(actorContext.UserId, request.Reason);
        await approvalRequestRepository.UpdateAsync(approvalRequest, cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(quotation.TenantId, "Quotation", quotation.Id, "Approved", "Quotation approval granted", new { approvalRequest.Id, approvalRequest.RevisionId, approvalRequest.DecisionReason }, actorContext.UserId),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
