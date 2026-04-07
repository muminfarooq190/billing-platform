using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.DeleteQuotationAttachment;

public sealed class DeleteQuotationAttachmentCommandHandler(
    IQuotationRepository quotationRepository,
    IQuotationAttachmentRepository quotationAttachmentRepository,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteQuotationAttachmentCommand>
{
    public async Task Handle(DeleteQuotationAttachmentCommand request, CancellationToken cancellationToken)
    {
        var quotation = await quotationRepository.GetByIdAsync(request.QuotationId, cancellationToken)
            ?? throw new DomainException($"Quotation {request.QuotationId} not found.");

        if (quotation.TenantId != request.TenantId)
            throw new DomainException("Quotation does not belong to the active tenant.");

        var attachment = await quotationAttachmentRepository.GetByIdAsync(request.QuotationId, request.AttachmentId, cancellationToken)
            ?? throw new DomainException("Quotation attachment not found.");

        if (attachment.TenantId != request.TenantId)
            throw new DomainException("Quotation attachment does not belong to the active tenant.");

        attachment.Delete();
        await quotationAttachmentRepository.UpdateAsync(attachment, cancellationToken);
        await fileStorage.DeleteAsync(attachment.StorageKey, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
