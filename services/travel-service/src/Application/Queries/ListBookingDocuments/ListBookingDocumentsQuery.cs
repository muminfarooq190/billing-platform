using MediatR;

namespace TravelService.Application.Queries.ListBookingDocuments;

public sealed record ListBookingDocumentsQuery(Guid TenantId, Guid BookingId) : IRequest<IReadOnlyList<BookingDocumentReadModel>>;

public sealed record BookingDocumentReadModel(
    Guid Id,
    Guid BookingId,
    Guid? TravelerId,
    Guid TenantId,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    string DocumentType,
    bool IsCustomerVisible,
    string? Description,
    string ReadUrl,
    DateTimeOffset CreatedAt);
