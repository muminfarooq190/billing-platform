using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.AcceptQuotation;

public sealed class AcceptQuotationCommandHandler(
    IQuotationRepository quotationRepository,
    IQuotationRevisionRepository quotationRevisionRepository,
    IQuotationStatusHistoryRepository quotationStatusHistoryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AcceptQuotationCommand>
{
    public async Task Handle(AcceptQuotationCommand request, CancellationToken cancellationToken)
    {
        var quotation = await quotationRepository.GetByIdAsync(request.QuotationId, cancellationToken)
            ?? throw new DomainException($"Quotation {request.QuotationId} not found.");

        if (quotation.TenantId != request.TenantId)
            throw new DomainException("Quotation does not belong to the active tenant.");

        var revision = await quotationRevisionRepository.GetByIdAsync(request.QuotationId, request.RevisionId, cancellationToken)
            ?? throw new DomainException("Quotation revision not found.");

        if (revision.TenantId != request.TenantId)
            throw new DomainException("Quotation revision does not belong to the active tenant.");

        var previousStatus = quotation.Status.ToString();
        quotation.Accept(revision.Id);

        await quotationStatusHistoryRepository.AddAsync(
            QuotationStatusHistory.Create(quotation.Id, quotation.TenantId, previousStatus, quotation.Status.ToString(), request.Reason),
            cancellationToken);

        await quotationRepository.UpdateAsync(quotation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
