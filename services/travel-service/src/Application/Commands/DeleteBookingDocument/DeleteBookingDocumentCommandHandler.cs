using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.DeleteBookingDocument;

public sealed class DeleteBookingDocumentCommandHandler(
    IBookingRepository bookingRepository,
    IBookingDocumentRepository bookingDocumentRepository,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteBookingDocumentCommand>
{
    public async Task Handle(DeleteBookingDocumentCommand request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new DomainException($"Booking {request.BookingId} not found.");

        if (booking.TenantId != request.TenantId)
            throw new DomainException("Booking does not belong to the active tenant.");

        var document = await bookingDocumentRepository.GetByIdAsync(request.BookingId, request.DocumentId, cancellationToken)
            ?? throw new DomainException("Booking document not found.");

        if (document.TenantId != request.TenantId)
            throw new DomainException("Booking document does not belong to the active tenant.");

        document.Delete();
        await bookingDocumentRepository.UpdateAsync(document, cancellationToken);
        await fileStorage.DeleteAsync(document.StorageKey, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
