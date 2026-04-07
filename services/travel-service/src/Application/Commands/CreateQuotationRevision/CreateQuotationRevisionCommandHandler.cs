using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.CreateQuotationRevision;

public sealed class CreateQuotationRevisionCommandHandler(
    IQuotationRepository quotationRepository,
    IQuotationRevisionRepository quotationRevisionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateQuotationRevisionCommand, CreateQuotationRevisionResult>
{
    public async Task<CreateQuotationRevisionResult> Handle(CreateQuotationRevisionCommand request, CancellationToken cancellationToken)
    {
        var quotation = await quotationRepository.GetByIdAsync(request.QuotationId, cancellationToken)
            ?? throw new DomainException($"Quotation {request.QuotationId} not found.");

        if (quotation.TenantId != request.TenantId)
            throw new DomainException("Quotation does not belong to the active tenant.");

        quotation.ReplaceDraftDetails(
            request.Title,
            request.Destination,
            request.TravelDate,
            request.ReturnDate,
            request.Travellers,
            request.Currency,
            request.VisibleNotes,
            request.ValidUntil);

        quotation.ReplaceLineItems(request.LineItems.Select(x => (x.Description, x.UnitPrice, x.Quantity, x.Currency)));

        var revision = quotation.CreateRevision(request.VisibleNotes, request.InternalNotes);

        await quotationRevisionRepository.AddAsync(revision, cancellationToken);
        await quotationRepository.UpdateAsync(quotation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateQuotationRevisionResult(revision.Id, revision.RevisionNumber);
    }
}
