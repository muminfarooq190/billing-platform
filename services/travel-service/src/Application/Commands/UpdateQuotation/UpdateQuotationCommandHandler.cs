using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;
using MediatR;

namespace TravelService.Application.Commands.UpdateQuotation;

public sealed class UpdateQuotationCommandHandler(
    IQuotationRepository quotationRepository,
    IQuotationStatusHistoryRepository quotationStatusHistoryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateQuotationCommand>
{
    public async Task Handle(UpdateQuotationCommand request, CancellationToken cancellationToken)
    {
        var quotation = await quotationRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException($"Quotation {request.Id} not found.");

        var shouldWriteHistory = false;
        var previousStatus = quotation.Status.ToString();

        switch (request.Action?.ToLowerInvariant())
        {
            case "send":
                quotation.Send();
                shouldWriteHistory = true;
                break;
            case "accept":
            case "reject":
                throw new DomainException("Use the dedicated quotation status endpoints for accept/reject actions.");
            default:
                quotation.Update(request.Title, request.Destination, request.TravelDate, request.ReturnDate, request.Travellers, request.Currency, request.Notes, request.ValidUntil);
                break;
        }

        if (shouldWriteHistory)
        {
            await quotationStatusHistoryRepository.AddAsync(
                QuotationStatusHistory.Create(quotation.Id, quotation.TenantId, previousStatus, quotation.Status.ToString(), null),
                cancellationToken);
        }

        await quotationRepository.UpdateAsync(quotation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
