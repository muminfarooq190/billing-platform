using MediatR;

namespace TravelService.Application.Commands.UploadBookingDocument;

public sealed record UploadBookingDocumentCommand(
    Guid TenantId,
    Guid BookingId,
    Guid? TravelerId,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    string DocumentType,
    bool IsCustomerVisible,
    string? Description,
    byte[] Content) : IRequest<UploadBookingDocumentResult>;

public sealed record UploadBookingDocumentResult(Guid DocumentId);
