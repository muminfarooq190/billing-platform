using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.RejectQuotation;

public sealed class RejectQuotationCommandHandler(
    IQuotationRepository quotationRepository,
    IQuotationStatusHistoryRepository quotationStatusHistoryRepository,
    IAuditWriter auditWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<RejectQuotationCommand>
{
    public async Task Handle(RejectQuotationCommand request, CancellationToken cancellationToken)
    {
        var quotation = await quotationRepository.GetByIdAsync(request.QuotationId, cancellationToken)
            ?? throw new DomainException($"Quotation {request.QuotationId} not found.");

        if (quotation.TenantId != request.TenantId)
            throw new DomainException("Quotation does not belong to the active tenant.");

        var previousStatus = quotation.Status.ToString();
        quotation.Reject();

        await quotationStatusHistoryRepository.AddAsync(
            QuotationStatusHistory.Create(quotation.Id, quotation.TenantId, previousStatus, quotation.Status.ToString(), request.Reason),
            cancellationToken);

        await quotationRepository.UpdateAsync(quotation, cancellationToken);
        await auditWriter.WriteAsync(
            AuditLog.Create(
                quotation.TenantId,
                "Quotation",
                quotation.Id,
                "Rejected",
                actorContext.UserId,
                actorContext.IpAddress,
                actorContext.UserAgent,
                before: new { Status = previousStatus, quotation.RejectedAt },
                after: new { Status = quotation.Status.ToString(), quotation.RejectedAt },
                metadata: new { request.Reason }),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
