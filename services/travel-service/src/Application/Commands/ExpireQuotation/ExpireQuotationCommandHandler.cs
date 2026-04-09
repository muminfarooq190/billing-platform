using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.ExpireQuotation;

public sealed class ExpireQuotationCommandHandler(
    IQuotationRepository quotationRepository,
    IQuotationStatusHistoryRepository quotationStatusHistoryRepository,
    IAuditWriter auditWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<ExpireQuotationCommand>
{
    public async Task Handle(ExpireQuotationCommand request, CancellationToken cancellationToken)
    {
        var quotation = await quotationRepository.GetByIdAsync(request.QuotationId, cancellationToken)
            ?? throw new DomainException($"Quotation {request.QuotationId} not found.");

        if (quotation.TenantId != request.TenantId)
            throw new DomainException("Quotation does not belong to the active tenant.");

        var previousStatus = quotation.Status.ToString();
        quotation.Expire();

        await quotationStatusHistoryRepository.AddAsync(
            QuotationStatusHistory.Create(quotation.Id, quotation.TenantId, previousStatus, quotation.Status.ToString(), request.Reason),
            cancellationToken);

        await quotationRepository.UpdateAsync(quotation, cancellationToken);
        await auditWriter.WriteAsync(
            AuditLog.Create(
                quotation.TenantId,
                "Quotation",
                quotation.Id,
                "Expired",
                actorContext.UserId,
                actorContext.IpAddress,
                actorContext.UserAgent,
                before: new { Status = previousStatus, quotation.ExpiredAt },
                after: new { Status = quotation.Status.ToString(), quotation.ExpiredAt },
                metadata: new { request.Reason }),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
