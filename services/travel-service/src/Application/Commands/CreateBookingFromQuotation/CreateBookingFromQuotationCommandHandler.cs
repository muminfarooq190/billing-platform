using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.CreateBookingFromQuotation;

public sealed class CreateBookingFromQuotationCommandHandler(
    IQuotationRepository quotationRepository,
    IQuotationRevisionRepository quotationRevisionRepository,
    IBookingRepository bookingRepository,
    IBookingStatusHistoryRepository bookingStatusHistoryRepository,
    IFeatureGate featureGate,
    IActivityWriter activityWriter,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateBookingFromQuotationCommand, CreateBookingFromQuotationResult>
{
    public async Task<CreateBookingFromQuotationResult> Handle(CreateBookingFromQuotationCommand request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.TravelBookingCreate, request.TenantId, cancellationToken);

        var quotation = await quotationRepository.GetByIdAsync(request.QuotationId, cancellationToken)
            ?? throw new DomainException($"Quotation {request.QuotationId} not found.");

        if (quotation.TenantId != request.TenantId)
            throw new DomainException("Quotation does not belong to the active tenant.");

        if (quotation.AcceptedRevisionId is null || quotation.AcceptedRevisionId == Guid.Empty)
            throw new DomainException("Booking can only be created from an accepted quotation revision.");

        var revision = await quotationRevisionRepository.GetByIdAsync(quotation.Id, quotation.AcceptedRevisionId.Value, cancellationToken)
            ?? throw new DomainException("Accepted quotation revision not found.");

        if (revision.TenantId != request.TenantId)
            throw new DomainException("Accepted quotation revision does not belong to the active tenant.");

        var existingBooking = await bookingRepository.GetByAcceptedRevisionIdAsync(revision.Id, cancellationToken);
        if (existingBooking is not null)
            throw new DomainException("A booking already exists for this accepted quotation revision.");

        var bookingNumber = $"VOY-BKG-{DateTimeOffset.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        var booking = Booking.CreateFromAcceptedQuotation(
            quotation.TenantId,
            quotation.Id,
            revision.Id,
            quotation.CustomerContactId,
            bookingNumber,
            revision.Title,
            revision.Destination,
            revision.TravelDate,
            revision.ReturnDate,
            revision.Travellers,
            revision.Currency,
            revision.TotalAmount,
            request.AssignedToUserId,
            null,
            request.InternalNotes);

        await bookingRepository.AddAsync(booking, cancellationToken);
        await bookingStatusHistoryRepository.AddAsync(BookingStatusHistory.Create(booking.Id, booking.TenantId, null, booking.Status.ToString(), "Created from accepted quotation."), cancellationToken);
        await activityWriter.WriteAsync(ActivityEntry.Create(booking.TenantId, "Booking", booking.Id, "BookingCreated", $"Booking {booking.BookingNumber} created from accepted quotation", new { booking.QuotationId, booking.AcceptedRevisionId, booking.Status }), cancellationToken);
        await activityWriter.WriteAsync(ActivityEntry.Create(quotation.TenantId, "Quotation", quotation.Id, "BookingCreated", $"Booking {booking.BookingNumber} created from accepted quotation", new { booking.Id, booking.BookingNumber, booking.Status }), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateBookingFromQuotationResult(booking.Id, booking.BookingNumber);
    }
}
