using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.UploadBookingDocument;

public sealed class UploadBookingDocumentCommandHandler(
    IBookingRepository bookingRepository,
    ITravelerRepository travelerRepository,
    IBookingDocumentRepository bookingDocumentRepository,
    IFileStorage fileStorage,
    IActivityWriter activityWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<UploadBookingDocumentCommand, UploadBookingDocumentResult>
{
    private static readonly HashSet<string> AllowedContentTypes = ["application/pdf", "image/jpeg", "image/png", "image/webp"];

    public async Task<UploadBookingDocumentResult> Handle(UploadBookingDocumentCommand request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new DomainException($"Booking {request.BookingId} not found.");

        if (booking.TenantId != request.TenantId)
            throw new DomainException("Booking does not belong to the active tenant.");

        if (request.TravelerId.HasValue)
        {
            var traveler = await travelerRepository.GetByIdAsync(request.BookingId, request.TravelerId.Value, cancellationToken)
                ?? throw new DomainException("Traveler not found for booking document.");

            if (traveler.TenantId != request.TenantId)
                throw new DomainException("Traveler does not belong to the active tenant.");
        }

        var contentType = request.ContentType?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!AllowedContentTypes.Contains(contentType))
            throw new DomainException($"Document content type '{request.ContentType}' is not allowed.");

        var extension = Path.GetExtension(request.OriginalFileName);
        var storageKey = $"tenant/{request.TenantId:D}/bookings/{request.BookingId:D}/documents/{Guid.NewGuid():N}{extension}";
        await using var stream = new MemoryStream(request.Content);
        var uploadedStorageKey = await fileStorage.UploadAsync(stream, storageKey, contentType, cancellationToken);

        var document = BookingDocument.Create(
            request.BookingId,
            request.TravelerId,
            request.TenantId,
            uploadedStorageKey,
            request.OriginalFileName,
            contentType,
            request.SizeBytes,
            request.DocumentType,
            request.IsCustomerVisible,
            request.Description);

        await bookingDocumentRepository.AddAsync(document, cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                request.TenantId,
                "Booking",
                booking.Id,
                "DocumentUploaded",
                $"Booking document uploaded: {document.OriginalFileName}",
                new { document.Id, document.DocumentType, document.TravelerId, document.IsCustomerVisible },
                actorContext.UserId),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new UploadBookingDocumentResult(document.Id);
    }
}
